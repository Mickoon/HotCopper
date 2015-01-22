using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace HotCopper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitbutton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        // Hot Copper Posts, Threads and Authors
        private void button1_Click(object sender, EventArgs e)
        {
            var db = new FinanceCrawlerEntities();
            int duplicates = 0;
            int newData = 0;
            int postNewData = 0;

            // Retreive a source code from a webpage
            string url = textBox1.Text;         // e.g. http://hotcopper.com.au/asx/anz#.VI98gSuUfJI

            string stock = textBox2.Text;
            if (stock == null || stock.Trim() == "")
            {
                int identifier = url.IndexOf("hotcopper.com.au") + 16;
                stock = url.Substring(identifier, url.Length - identifier);
                if (stock.Contains("#")) stock = stock.Substring(0, stock.IndexOf("#"));
            }

            if (url != null && url.Trim() != "")
            {
                try
                {
                    string sourceCode = WorkerClasses.getSourceCode(url);
                    if (sourceCode == "invalid") throw new UriFormatException();
                    listbox.Items.Add("Process Starts. Please wait for a few minutes.");

                    /* Group */
                    string groupWord = textBox2.Text;
                    if (groupWord == "")
                        groupWord = WorkerClasses.getGroupWord(url);

                    #region HotCopper THREADS results only
                    try
                    {
                        /* First of ALL, Save Threads Links */
                        while (sourceCode.IndexOf("listblock tags") != -1)
                        {
                            // TAG
                            int startIndex = sourceCode.IndexOf("listblock tags");
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            int endIndex = sourceCode.IndexOf("</div");
                            string relCode = sourceCode.Substring(0, endIndex);
                            string tags = "";
                            while (relCode.Contains("<a href"))
                            {
                                startIndex = relCode.IndexOf("<a href");
                                relCode = relCode.Substring(startIndex, relCode.Length - startIndex);
                                startIndex = relCode.IndexOf(">") + 1;
                                relCode = relCode.Substring(startIndex, relCode.Length - startIndex);
                                endIndex = relCode.IndexOf("</");
                                tags += " " + relCode.Substring(0, endIndex);
                            }
                            tags = tags.Replace("&amp;", "&").Trim();

                            // Subject
                            startIndex = sourceCode.IndexOf("listblock subject");
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf("<h");
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf(">") + 1;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            endIndex = sourceCode.IndexOf("</");
                            string subject = sourceCode.Substring(0, endIndex);
                            string threadLink = "";
                            if (subject.Contains("<a"))
                            {
                                // Thread Link
                                string tempCode = sourceCode;
                                startIndex = tempCode.IndexOf("<a href");
                                tempCode = tempCode.Substring(startIndex, tempCode.Length - startIndex);
                                startIndex = tempCode.IndexOf("\"") + 1;
                                tempCode = tempCode.Substring(startIndex, tempCode.Length - startIndex);
                                endIndex = tempCode.IndexOf("\"");
                                threadLink = "http://hotcopper.com.au/" + tempCode.Substring(0, endIndex);

                                // Remove "<a>" from Subject
                                endIndex = subject.IndexOf("<a");
                                string firstPart = subject.Substring(0, endIndex);
                                subject = subject.Substring(endIndex + 2, subject.Length - (endIndex + 2));
                                startIndex = subject.IndexOf(">") + 1;
                                string secondPart = subject.Substring(startIndex, subject.Length - startIndex);
                                subject = firstPart + "\n" + secondPart;
                            }
                            subject = subject.Replace("</a>", "").Trim();
                            subject = subject.Replace("&#039;", "'");
                            subject = subject.Replace("&amp;", "'");

                            // Author
                            startIndex = sourceCode.IndexOf("listblock author");
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf(">") + 1;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            endIndex = sourceCode.IndexOf("</");
                            string author = sourceCode.Substring(0, endIndex);
                            // Author's Posts Link
                            startIndex = sourceCode.IndexOf("a href");
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf("\"") + 1;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            endIndex = sourceCode.IndexOf("\"");
                            string authorPostLink = "http://hotcopper.com.au/" + sourceCode.Substring(0, endIndex);
                            authorPostLink = authorPostLink.Replace("&amp;", "&");
                            while (author.Contains("<a"))
                            {
                                endIndex = author.IndexOf("<a");
                                string firstPart = author.Substring(0, endIndex);
                                author = author.Substring(endIndex + 2, author.Length - (endIndex + 2));
                                startIndex = author.IndexOf(">") + 1;
                                string secondPart = author.Substring(startIndex, author.Length - startIndex);
                                author = firstPart + "\n" + secondPart;
                            }
                            author = author.Replace("</a>", "").Trim();
                            author = author.Replace("&#039;", "'");

                            // Check the subject if it is not a reply and not an announcement  (First Post only)
                            if (!subject.Substring(0, 4).ToLower().Contains("re:") && !subject.Substring(0, 4).ToLower().Contains("ann:"))
                            {
                                // Views
                                startIndex = sourceCode.IndexOf("listblock stats");
                                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                                startIndex = sourceCode.IndexOf("</span>") + 7;
                                if (startIndex == -1) startIndex = sourceCode.IndexOf(">") + 1;
                                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                                endIndex = sourceCode.IndexOf("</");
                                string viewStr = sourceCode.Substring(0, endIndex).Trim();
                                viewStr = viewStr.Replace(",", "");
                                int view = Convert.ToInt32(viewStr);

                                //// Immediate DateTime
                                startIndex = sourceCode.IndexOf("listblock time");
                                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                                startIndex = sourceCode.IndexOf(">") + 1;
                                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                                startIndex = sourceCode.IndexOf(">") + 1;
                                endIndex = sourceCode.IndexOf("</");
                                if (startIndex < endIndex && startIndex != -1) 
                                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                                endIndex = sourceCode.IndexOf("</");
                                string datetimeStr = sourceCode.Substring(0, endIndex).Trim();
                                DateTime threadDate = Convert.ToDateTime(datetimeStr);

                                // Access inside the thread
                                if (threadLink != "")
                                {
                                    string threadCode = WorkerClasses.getSourceCode(threadLink);
                                    if (threadCode == "invalid") throw new UriFormatException();

                                    // Date of the thread
                                    startIndex = threadCode.IndexOf("icon left icon-clock");
                                    threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                    startIndex = threadCode.IndexOf("</span>") + 7;
                                    threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                    endIndex = threadCode.IndexOf("</");
                                    string threadBeginStr = threadCode.Substring(0, endIndex).Trim();
                                    DateTime threadBegin = Convert.ToDateTime(datetimeStr);

                                    // if Nav to First does not exist, this post is the last one, save as it is.
                                    int totalPosts = 1;
                                    string lastPoster = author;
                                    string lastPost = threadLink;
                                    string threadCodeOrg = threadCode;

                                    startIndex = threadCode.IndexOf("rel=\"start\"");   // Nav to First
                                    // if first exists, Check 'PageNav', find a total number, then go to last post, 
                                    //   find save last post link and last poster.

                                    //................................. UPDATE AREA ................................//
                                    if (startIndex != -1)
                                    {
                                        // Total Number of posts 
                                        startIndex = threadCode.IndexOf("PageNav");
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        startIndex = threadCode.IndexOf("data-last") + 9;
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        startIndex = threadCode.IndexOf("\"") + 1;
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        endIndex = threadCode.IndexOf("\"");
                                        totalPosts = Convert.ToInt32(threadCode.Substring(0, endIndex));

                                        // ------:> Navigate to Last Post  
                                        startIndex = threadCode.IndexOf("Next</a>");
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        startIndex = threadCode.IndexOf("a href");
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        startIndex = threadCode.IndexOf("\"") + 1;
                                        threadCode = threadCode.Substring(startIndex, threadCode.Length - startIndex);
                                        endIndex = threadCode.IndexOf("\"");
                                        string lastPostLink = "http://hotcopper.com.au/" + threadCode.Substring(0, endIndex);
                                        string lastPostCode = WorkerClasses.getSourceCode(lastPostLink);
                                        if (lastPostCode == "invalid") throw new UriFormatException();

                                        // Last Post
                                        lastPost = lastPostLink;

                                        // Last Poster
                                        startIndex = lastPostCode.IndexOf("user-wrap");
                                        lastPostCode = lastPostCode.Substring(startIndex, lastPostCode.Length - startIndex);
                                        startIndex = lastPostCode.IndexOf("<h");
                                        lastPostCode = lastPostCode.Substring(startIndex, lastPostCode.Length - startIndex);
                                        startIndex = lastPostCode.IndexOf(">") + 1;
                                        lastPostCode = lastPostCode.Substring(startIndex, lastPostCode.Length - startIndex);
                                        startIndex = lastPostCode.IndexOf(">") + 1;
                                        endIndex = lastPostCode.IndexOf("</");
                                        lastPoster = lastPostCode.Substring(0, endIndex);
                                        while (lastPoster.Contains("<a"))
                                        {
                                            endIndex = lastPoster.IndexOf("<a");
                                            string firstPart = lastPoster.Substring(0, endIndex);
                                            lastPoster = lastPoster.Substring(endIndex + 2, lastPoster.Length - (endIndex + 2));
                                            startIndex = lastPoster.IndexOf(">") + 1;
                                            string secondPart = lastPoster.Substring(startIndex, lastPoster.Length - startIndex);
                                            lastPoster = firstPart + "\n" + secondPart;
                                        }
                                        lastPoster = lastPoster.Replace("</a>", "").Trim();
                                        lastPoster = lastPoster.Replace("&#039;", "'");
                                    }

                                    if (!db.HotCopper_Threads.Any(f => f.Subject == subject && f.Begin_Date == threadBegin))
                                    {
                                        db.HotCopper_Threads.Add(new HotCopper_Threads
                                        {
                                            Stock = stock,
                                            Tags = tags,
                                            Subject = subject,
                                            Num_of_Posts = totalPosts,
                                            Num_of_Views = view,
                                            First_Poster = author,
                                            Begin_Date = threadBegin, 
                                            Last_Post = lastPost,
                                            Last_Poster = lastPoster
                                        });
                                        db.SaveChanges();
                                        newData++;
                                    }
                                    else
                                    {
                                        var existing = (from u in db.HotCopper_Threads 
                                                        where u.Subject == subject 
                                                        && u.Begin_Date == threadBegin select u).FirstOrDefault();
                                        if (existing != null)
                                        {
                                            if (existing.Last_Post != lastPost)
                                                existing.Last_Post = lastPost;
                                            if (existing.Last_Poster != lastPoster)
                                                existing.Last_Poster = lastPoster;
                                            if (existing.Num_of_Posts != totalPosts)
                                                existing.Num_of_Posts = totalPosts;
                                            if (existing.Num_of_Views != view)
                                                existing.Num_of_Views = view;
                                            db.SaveChanges();
                                            duplicates++;
                                        }
                                    }

                                    // Save Page Data
                                    postNewData += WorkerClasses.savePageData(stock, subject, threadCodeOrg, threadLink, authorPostLink);
                                }
                            }
                            // Second, Third, Forth and after posts...
                            else if (subject.Substring(0, 4).ToLower().Contains("re:"))
                            {
                                string threadCodeOrg = WorkerClasses.getSourceCode(threadLink);
                                if (threadCodeOrg == "invalid") throw new UriFormatException();

                                // Save Page Data
                                postNewData += WorkerClasses.savePageData(stock, subject, threadCodeOrg, threadLink, authorPostLink);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        listbox.Items.Add("Error: " + ex);
                    }

                    listbox.Items.Add("\n[" + DateTime.Now + "] HotCopper Threads completed.\n" + newData + " saved and " + duplicates + " duplicates Updated.");
                    listbox.Items.Add("\n[" + DateTime.Now + "] HotCopper Posts completed.\n" + postNewData + " saved.");
                    #endregion
                }
                catch (Exception)
                {
                    listbox.Items.Add("Invalid URL!");
                    MessageBox.Show("Invalid URL!");
                    textBox1.Text = "";
                }
            }
            else
            {
                listbox.Items.Add("Please enter URL.");
                MessageBox.Show("Please enter URL.");
            }
        }

        // MARKET DATA
        private void button3_Click(object sender, EventArgs e)
        {
            var db = new FinanceCrawlerEntities();
            int newData = 0;

            // Retreive a source code from a webpage
            string url = textBox1.Text;         // e.g. http://hotcopper.com.au/asx/anz#.VI98gSuUfJI
            if (url != null && url.Trim() != "")
            {
                try
                {
                    string sourceCode = WorkerClasses.getSourceCode(url);
                    if (sourceCode == "invalid") throw new UriFormatException();
                    listbox.Items.Add("[" + DateTime.Now + "] Process Starts. Please wait for a few minutes.");

                    /* TAG */
                    string groupWord = textBox2.Text;
                    if (groupWord == "")
                        groupWord = WorkerClasses.getGroupWord(url);

                    int startIndex = 0; int endIndex = 0;
                    startIndex = sourceCode.IndexOf("stock-pricing");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);

                    // High
                    startIndex = sourceCode.IndexOf("class=\"high\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    string temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    temp = temp.Replace("$", "");
                    if (temp.Contains(""))
                    {
                        temp = temp.Replace("&cent;", "");
                        temp = Convert.ToString(Convert.ToDouble(temp) / 100);
                    }
                    Decimal highValue = Convert.ToDecimal(temp);

                    // Low
                    startIndex = sourceCode.IndexOf("class=\"low\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    temp = temp.Replace("$", "");
                    if (temp.Contains(""))
                    {
                        temp = temp.Replace("&cent;", "");
                        temp = Convert.ToString(Convert.ToDouble(temp) / 100);
                    }
                    Decimal lowValue = Convert.ToDecimal(temp);

                    // Open
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    temp = temp.Replace("$", "");
                    if (temp.Contains(""))
                    {
                        temp = temp.Replace("&cent;", "");
                        temp = Convert.ToString(Convert.ToDouble(temp) / 100);
                    }
                    Decimal openValue = Convert.ToDecimal(temp);

                    // Last
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    temp = temp.Replace("$", "");
                    if (temp.Contains(""))
                    {
                        temp = temp.Replace("&cent;", "");
                        temp = Convert.ToString(Convert.ToDouble(temp) / 100);
                    }
                    Decimal lastValue = Convert.ToDecimal(temp);

                    // Market Price
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    temp = temp.Replace("$", "");
                    if (temp.Contains(""))
                    {
                        temp = temp.Replace("&cent;", "");
                        temp = Convert.ToString(Convert.ToDouble(temp) / 100);
                    }
                    Decimal marketPrice = Convert.ToDecimal(temp);

                    // Volume (Millions)
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf(">") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    if (temp.Contains("m") || temp.Contains("M"))
                        temp = temp.Replace("m", "").Replace("M", "");
                    else if (temp.Contains("b") || temp.Contains("B"))
                    {
                        temp = temp.Replace("b", "").Replace("B", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) * 1000);
                    }
                    else if (temp.Contains("k") || temp.Contains("K"))
                    {
                        temp = temp.Replace("k", "").Replace("K", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000);
                    }
                    else
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000000);
                    Decimal volume = Convert.ToDecimal(temp);

                    // Value (Millions)
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf("$") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    if (temp.Contains("m") || temp.Contains("M"))
                        temp = temp.Replace("m", "").Replace("M", "");
                    else if (temp.Contains("b") || temp.Contains("B"))
                    {
                        temp = temp.Replace("b", "").Replace("B", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) * 1000);
                    }
                    else if (temp.Contains("k") || temp.Contains("K"))
                    {
                        temp = temp.Replace("k", "").Replace("K", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000);
                    }
                    else
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000);
                    Decimal value = Convert.ToDecimal(temp);

                    // Market Cap (Billions)
                    startIndex = sourceCode.IndexOf("class=\"primary\"");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf("$") + 1;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</");
                    temp = sourceCode.Substring(0, endIndex).Replace(",", "");
                    if (temp.Contains("b") || temp.Contains("B"))
                        temp = temp.Replace("b", "").Replace("B", "");
                    else if (temp.Contains("m") || temp.Contains("M"))
                    {
                        temp = temp.Replace("m", "").Replace("M", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000);
                    }
                    else if (temp.Contains("k") || temp.Contains("K"))
                    {
                        temp = temp.Replace("k", "").Replace("K", "");
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000000);
                    }
                    else
                        temp = Convert.ToString(Convert.ToDecimal(temp) / 1000000000);
                    Decimal marketCap = Convert.ToDecimal(temp);

                    db.HotCopper_Market_data.Add(new HotCopper_Market_data
                    {
                        Tag = groupWord,
                        Date = DateTime.Now,
                        High = highValue,
                        Low = lowValue,
                        Open = openValue,
                        Last = lastValue,
                        Market_Price = marketPrice,
                        Volume__Millions_ = volume,
                        Value__Millions_ = value,
                        Market_Cap__Billions_ = marketCap
                    });
                    db.SaveChanges();
                    newData++;

                    listbox.Items.Add("\n[" + DateTime.Now + "] Market Data completed.\n" + newData + " saved.");
                }
                catch (UriFormatException)
                {
                    listbox.Items.Add("Invalid URL!");
                    MessageBox.Show("Invalid URL!");
                    textBox1.Text = "";
                }
                catch (Exception ex)
                {
                    listbox.Items.Add("Error found: " + ex);
                    MessageBox.Show("Error found: " + ex);
                    textBox1.Text = "";
                }
            }
        }

        // Positive and Negative words
        private void button2_Click(object sender, EventArgs e)
        {
            var db = new FinanceCrawlerEntities();
            int duplicates = 0;
            int updatedData = 0;

            // Retreive a source code from a webpage
            // e.g. http://www3.nd.edu/~mcdonald/Word_Lists.html
            try
            {
                listbox.Items.Add("Process Starts. Please wait for a few minutes.");
                string word = ""; int posCount = 0; int negCount = 0;

                // Positive Words
                string positiveWordLink = "http://www3.nd.edu/~mcdonald/Data/Finance_Word_Lists/LoughranMcDonald_Positive.csv";
                string positiveCode = WorkerClasses.getSourceCode(positiveWordLink).ToLower().Replace("\n", "");
                if (positiveCode == "invalid") throw new UriFormatException();
                string positiveCode_copy = positiveCode;

                // Negative Words
                string negativeWordLink = "http://www3.nd.edu/~mcdonald/Data/Finance_Word_Lists/LoughranMcDonald_Negative.csv";
                string negativeCode = WorkerClasses.getSourceCode(negativeWordLink).ToLower().Replace("\n", "");
                if (negativeCode == "invalid") throw new UriFormatException();
                string negativeCode_copy = negativeCode;

                // Check with the article's story if a word in the list is contained in a story
                //   if it is contained, count how many times.
                var postsList = (from u in db.HotCopper_Posts 
                                select u).ToList();
                if (postsList.Count != 0)
                {
                    foreach (var u in postsList)
                    {
                        if (u.PosWords == null && u.NegWords == null)
                        {
                            string content = u.Content;
                            string[] contentSplit = content.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            // Select a word (read by line) in the list
                            while (positiveCode.IndexOf("\r") != -1)
                            {
                                int endIndex = positiveCode.IndexOf("\r");
                                word = positiveCode.Substring(0, endIndex);
                                positiveCode = positiveCode.Substring(endIndex + 1, positiveCode.Length - endIndex - 1);
                                if (content.Contains(word))
                                {
                                    var matchQuery = from words in contentSplit
                                                     where words.ToLowerInvariant() == word.ToLowerInvariant()
                                                     select words;
                                    posCount += matchQuery.Count();
                                }
                            }
                            while (negativeCode.IndexOf("\r") != -1)
                            {
                                int endIndex = negativeCode.IndexOf("\r");
                                word = negativeCode.Substring(0, endIndex);
                                negativeCode = negativeCode.Substring(endIndex + 1, negativeCode.Length - endIndex - 1);
                                if (content.Contains(word))
                                {
                                    var matchQuery = from words in contentSplit
                                                     where words.ToLowerInvariant() == word.ToLowerInvariant()
                                                     select words;
                                    negCount += matchQuery.Count();
                                }
                            }

                            u.PosWords = posCount;
                            u.NegWords = negCount;
                            db.SaveChanges();
                            updatedData++;
                            
                            // Reset components for next loop
                            positiveCode = positiveCode_copy;
                            negativeCode = negativeCode_copy;
                            posCount = 0;
                            negCount = 0;
                        }
                        else
                            duplicates++;
                    }
                }

                listbox.Items.Add("\n[" + DateTime.Now + "] HotCopper Pos/Neg Word Process Ended.\n" + updatedData + " updated and " + duplicates + " duplicates Found.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UriFormatException"))
                {
                    listbox.Items.Add("Invalid URL!");
                    MessageBox.Show("Invalid URL!");
                }
                else
                    listbox.Items.Add(ex.Message);
            }  
        }

        // HOT COPPER POSTS, Threads, Authors... By Text file
        private void button4_Click(object sender, EventArgs e)
        {
            listbox.Items.Add("[" + DateTime.Now + "] HOT COPPER by text file begins! Please wait for a few seconds.");
            DialogResult result = openFileDialog1.ShowDialog();
            string links = "";
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    links = File.ReadAllText(file);
                }
                catch (IOException)
                {
                    listbox.Items.Add("ERROR: Not appropriate file selected!");
                }
            }

            // Retreive source code from a webpage
            StringReader strReader = new StringReader(links);
            while (true)
            {
                string url = strReader.ReadLine();
                if (url != null && url.Trim() != "")
                {
                    textBox1.Text = url;
                    button1_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("\n[" + DateTime.Now + "] Task Ended.");
                    break;
                }
            }
        }

        // Market Data by Text file
        private void button5_Click(object sender, EventArgs e)
        {
            listbox.Items.Add("[" + DateTime.Now + "] Hot Copper Market Data by text file begins! Please wait for a few seconds.");
            DialogResult result = openFileDialog1.ShowDialog();
            string links = "";
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    links = File.ReadAllText(file);
                }
                catch (IOException)
                {
                    listbox.Items.Add("ERROR: Not appropriate file selected!");
                }
            }

            // Retreive source code from a webpage
            StringReader strReader = new StringReader(links);
            while (true)
            {
                string url = strReader.ReadLine();
                if (url != null && url.Trim() != "")
                {
                    textBox1.Text = url;
                    button3_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("\n[" + DateTime.Now + "] Task Ended.");
                    break;
                }
            }
        }  
    }
}
