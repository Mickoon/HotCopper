using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace HotCopper
{
    class WorkerClasses
    {
        public static string getSourceCode(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string sourceCode = sr.ReadToEnd();
                sr.Close();
                resp.Close();
                return sourceCode;
            }
            catch
            {
                return "invalid";
            }
        }

        public static string getGroupWord(string url)
        {
            int endIndex = url.LastIndexOf("/");
            url = url.Substring(0, endIndex);
            int startIndex = url.LastIndexOf("/") + 1;
            string groupWord = url.Substring(startIndex, url.Length - startIndex);
            groupWord = groupWord.Replace("&nbsp;", " ");
            groupWord = groupWord.Replace("&#39;", "'");
            groupWord = groupWord.Replace("&quot;", "\"");
            groupWord = groupWord.Replace("&amp;", "&");
            groupWord = groupWord.Replace("%3A", ":");
            return groupWord.ToUpper();
        }

        public static int savePageData(string stock, string subject, string threadCodeOrg, string threadLink, string authorPostLink)
        {
            var db = new FinanceCrawlerEntities();
            int startIndex = 0; int endIndex = 0;

            // Page Number
            int pageNum = 1;
            if (subject.Substring(0, 4).ToLower().Contains("re:"))
            {
                startIndex = threadLink.IndexOf("page-") + 5;
                threadLink = threadLink.Substring(startIndex, threadLink.Length - startIndex);
                endIndex = threadLink.IndexOf("?");
                pageNum = Convert.ToInt32(threadLink.Substring(0, endIndex));
            }

            // Author Link
            startIndex = threadCodeOrg.IndexOf("class=\"avatar\"");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("a href");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("\"") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("\"");
            string authorLink = "http://hotcopper.com.au/" + threadCodeOrg.Substring(0, endIndex);
            if (authorLink.Trim() != "") saveAuthorData(authorLink, authorPostLink);
            // Author
            startIndex = threadCodeOrg.IndexOf("<h");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf(">") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</");
            string postAuthor = threadCodeOrg.Substring(0, endIndex);
            if (postAuthor.Contains("<a"))
            {
                endIndex = postAuthor.IndexOf("<a");
                string firstPart = postAuthor.Substring(0, endIndex);
                postAuthor = postAuthor.Substring(endIndex + 2, postAuthor.Length - (endIndex + 2));
                startIndex = postAuthor.IndexOf(">") + 1;
                string secondPart = postAuthor.Substring(startIndex, postAuthor.Length - startIndex);
                postAuthor = firstPart + "\n" + secondPart;
            }
            postAuthor = postAuthor.Replace("</a>", "").Trim();
            postAuthor = postAuthor.Replace("&#039;", "'");

            // Date, Time 
            startIndex = threadCodeOrg.IndexOf("Date:");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("<dd");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf(">") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</");
            string postDate = threadCodeOrg.Substring(0, endIndex);
            startIndex = threadCodeOrg.IndexOf("Time:");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("<dd");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf(">") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</");
            string postTime = threadCodeOrg.Substring(0, endIndex);
            string postDateTimeStr = postDate + " " + postTime;
            DateTime postDateTime = Convert.ToDateTime(postDateTimeStr);

            // Post ID 
            startIndex = threadCodeOrg.IndexOf("Post #:");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("<dd");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf(">") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</");
            string postIDstr = threadCodeOrg.Substring(0, endIndex);
            if (postIDstr.Contains("<a"))
            {
                endIndex = postIDstr.IndexOf("<a");
                string firstPart = postIDstr.Substring(0, endIndex);
                postIDstr = postIDstr.Substring(endIndex + 2, postIDstr.Length - (endIndex + 2));
                startIndex = postIDstr.IndexOf(">") + 1;
                string secondPart = postIDstr.Substring(startIndex, postIDstr.Length - startIndex);
                postIDstr = firstPart + "\n" + secondPart;
            }
            postIDstr = postIDstr.Replace("</a>", "").Trim();
            int postID = Convert.ToInt32(postIDstr);

            // IP
            string postIP = "Not Found";
            startIndex = threadCodeOrg.IndexOf("IP:");
            if (startIndex != -1)
            {
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                startIndex = threadCodeOrg.IndexOf("<dd");
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                startIndex = threadCodeOrg.IndexOf(">") + 1;
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                endIndex = threadCodeOrg.IndexOf("</");
                postIP = threadCodeOrg.Substring(0, endIndex);
            }


            // CONTENT
            #region Content
            startIndex = threadCodeOrg.IndexOf("class=\"content\"");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("<article>") + 9;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("<blockquote");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf(">") + 1;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</blockquote");
            string story = threadCodeOrg.Substring(0, endIndex);
            while (story.Contains("<a"))
            {
                endIndex = story.IndexOf("<a");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 2, story.Length - (endIndex + 2));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<p"))
            {
                endIndex = story.IndexOf("<p");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 2, story.Length - (endIndex + 2));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<img"))
            {
                endIndex = story.IndexOf("<img");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 4, story.Length - (endIndex + 4));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<span"))
            {
                endIndex = story.IndexOf("<span");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 5, story.Length - (endIndex + 5));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<script"))
            {
                endIndex = story.IndexOf("<script");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 6, story.Length - (endIndex + 6));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<ins"))
            {
                endIndex = story.IndexOf("<ins");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 4, story.Length - (endIndex + 4));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<div"))
            {
                endIndex = story.IndexOf("<div");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 4, story.Length - (endIndex + 4));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<link"))
            {
                endIndex = story.IndexOf("<link");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 5, story.Length - (endIndex + 5));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<video"))
            {
                endIndex = story.IndexOf("<video");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 6, story.Length - (endIndex + 6));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<source"))
            {
                endIndex = story.IndexOf("<source");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 7, story.Length - (endIndex + 7));
                startIndex = story.IndexOf(">") + 1;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<style>"))
            {
                endIndex = story.IndexOf("<style>");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 7, story.Length - (endIndex + 7));
                startIndex = story.IndexOf("</style>") + 8;
                string secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            while (story.Contains("<!--"))
            {
                endIndex = story.IndexOf("<!--");
                string firstPart = story.Substring(0, endIndex);
                story = story.Substring(endIndex + 3, story.Length - (endIndex + 3));
                startIndex = story.IndexOf("-->") + 3;
                string secondPart = "";
                if (story.IndexOf("-->") != -1) secondPart = story.Substring(startIndex, story.Length - startIndex);
                story = firstPart + "\n" + secondPart;
            }
            story = story.Replace("</style>", "");
            story = story.Replace("</span>", "");
            story = story.Replace("</script>", "");
            story = story.Replace("</video>", "");
            story = story.Replace("</a>", "");
            story = story.Replace("<br />", "");
            story = story.Replace("</ul>", "");
            story = story.Replace("</li>", "");
            story = story.Replace("</tr>", "");
            story = story.Replace("</td>", "");
            story = story.Replace("<p>", "");
            story = story.Replace("</p>", "");
            story = story.Replace("<P>", "");
            story = story.Replace("</P>", "");
            story = story.Replace("</ins>", "");
            story = story.Replace("</div>", "");
            story = story.Replace("<em>", "");
            story = story.Replace("</em>", "");
            story = story.Replace("&nbsp;", " ");
            story = story.Replace("&lsquo;", "'");
            story = story.Replace("&rsquo;", "'");
            story = story.Replace("&ldquo;", "\"");
            story = story.Replace("&rdquo;", "\"");
            story = story.Replace("&quot;", "\"");
            story = story.Replace("&amp;", "&");
            story = story.Replace("&#8217;", "'");
            story = story.Replace("&#8220;", "\"");
            story = story.Replace("&#8221;", "\"");
            story = story.Replace("&thinsp;&#8212;&thinsp;", " - ");
            story = story.Replace("\n", "");
            story = story.Replace("\t", "");
            story = story.Replace("&#039;", "'");
            story = story.Replace("<b>", "");
            story = story.Replace("</b>", "");
            story = story.Replace("<br>", "");
            story = story.Replace("<strong>", "");
            story = story.Replace("</strong>", "").Trim();
            #endregion

            // Length of the post (Word Count - compared with MS word)
            //int postWordCount1 = Regex.Matches(story, @"[A-Za-z0-9]+").Count;
            int postWordCount = Regex.Matches(story, @"[\S]+").Count;      // Best Method so far
            //int postWordCount3 = story.Split().Length;
            //int postWordCount4 = story.Count(Char.IsWhiteSpace);    // -1
            //int postWordCount = Math.Min(Math.Min(postWordCount1, postWordCount2), Math.Min(postWordCount3, postWordCount4));

            // Likes
            startIndex = threadCodeOrg.IndexOf("icon icon-like");
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            startIndex = threadCodeOrg.IndexOf("</span>") + 7;
            threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
            endIndex = threadCodeOrg.IndexOf("</");
            string likesStr = threadCodeOrg.Substring(0, endIndex);
            likesStr = likesStr.Replace(",", "");
            int likes = Convert.ToInt32(likesStr);

            // Price at Posting 
            Decimal priceAtPosting = 0;
            startIndex = threadCodeOrg.IndexOf("Price at posting");
            if (startIndex != -1)
            {
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                startIndex = threadCodeOrg.IndexOf("$") + 1;
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                endIndex = threadCodeOrg.IndexOf("</");
                if (!threadCodeOrg.Substring(0, endIndex).Contains("("))
                    priceAtPosting = Convert.ToDecimal(threadCodeOrg.Substring(0, endIndex));
            }

            // Sentiment
            string sentiment = "Not Found";
            startIndex = threadCodeOrg.IndexOf("Sentiment");
            if (startIndex != -1)
            {
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                startIndex = threadCodeOrg.IndexOf("<dd>") + 4;
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                endIndex = threadCodeOrg.IndexOf("</");
                sentiment = threadCodeOrg.Substring(0, endIndex);
            }

            // Disclosure
            string disclosure = "Not Found";
            startIndex = threadCodeOrg.IndexOf("Disclosure");
            if (startIndex != -1)
            {
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                startIndex = threadCodeOrg.IndexOf("<dd>") + 4;
                threadCodeOrg = threadCodeOrg.Substring(startIndex, threadCodeOrg.Length - startIndex);
                endIndex = threadCodeOrg.IndexOf("</");
                disclosure = threadCodeOrg.Substring(0, endIndex);
            }

            if (!db.HotCopper_Posts.Any(f => f.Subject == subject && f.PageNum == pageNum && f.Post_ID == postID))
            {
                db.HotCopper_Posts.Add(new HotCopper_Posts
                {
                    Stock = stock,
                    Subject = subject,
                    PageNum = pageNum,
                    Content = story,
                    Likes = likes,
                    DateTime = postDateTime,
                    Author = postAuthor,
                    Post_ID = postID,
                    IP = postIP,
                    Length_of_Post = postWordCount,
                    Price_at_Posting = priceAtPosting,
                    Disclosure = disclosure,
                    Sentiment = sentiment
                });
                db.SaveChanges();
                return 1;
            }
            else
            {
                var existing = (from u in db.HotCopper_Posts
                                where u.Subject == subject
                                && u.PageNum == pageNum
                                && u.Post_ID == postID
                                select u).FirstOrDefault();
                if (existing != null)
                {
                    if (existing.Likes != likes)
                        existing.Likes = likes;
                    if (existing.Price_at_Posting != priceAtPosting)
                        existing.Price_at_Posting = priceAtPosting;
                    db.SaveChanges();
                }
                return 0;
            }
        }

        public static void saveAuthorData(string authorLink, string authorPostLink)
        {
            var db = new FinanceCrawlerEntities();
            try
            {
                string sourceCode = WorkerClasses.getSourceCode(authorLink);
                if (sourceCode == "invalid") throw new UriFormatException();

                int startIndex = 0; int endIndex = 0;
                // Name
                startIndex = sourceCode.IndexOf("itemprop=\"name");
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                startIndex = sourceCode.IndexOf(">") + 1;
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                endIndex = sourceCode.IndexOf("</");
                string authorName = sourceCode.Substring(0, endIndex);

                // Posts
                startIndex = sourceCode.IndexOf("stat-post");
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                startIndex = sourceCode.IndexOf("</div>") + 6;
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                endIndex = sourceCode.IndexOf("<");
                string postsNumStr = sourceCode.Substring(0, endIndex).Trim().Replace(",", "");
                int postsTotalNum = Convert.ToInt32(postsNumStr);

                // Likes Received
                startIndex = sourceCode.IndexOf("stat-likes");
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                startIndex = sourceCode.IndexOf("</div>") + 6;
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                endIndex = sourceCode.IndexOf("<");
                string likesReceivedStr = sourceCode.Substring(0, endIndex).Trim().Replace(",", "");
                int likesReceived = Convert.ToInt32(likesReceivedStr);

                // Following
                // Its Number
                startIndex = sourceCode.IndexOf("stat-following");
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                startIndex = sourceCode.IndexOf("</div>") + 6;
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                endIndex = sourceCode.IndexOf("<");
                string followingStr = sourceCode.Substring(0, endIndex).Trim().Replace(",", "");
                int following = Convert.ToInt32(followingStr);

                // Followers
                // Its Number
                startIndex = sourceCode.IndexOf("stat-followers");
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                startIndex = sourceCode.IndexOf("</div>") + 6;
                sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                endIndex = sourceCode.IndexOf("<");
                string followersStr = sourceCode.Substring(0, endIndex).Trim().Replace(",", "");
                int followers = Convert.ToInt32(followersStr);

                // Following Stocks stockList 
                startIndex = sourceCode.IndexOf("member-stockList");
                string stockLists = "";
                if (startIndex != -1)
                {
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</ol");
                    string stockCode = sourceCode.Substring(0, endIndex);
                    int count = 0;
                    while (stockCode.IndexOf("<li>") != -1)
                    {
                        startIndex = stockCode.IndexOf("<li>") + 4;
                        stockCode = stockCode.Substring(startIndex, stockCode.Length - startIndex);
                        startIndex = stockCode.IndexOf("title=") + 6;
                        stockCode = stockCode.Substring(startIndex, stockCode.Length - startIndex);
                        startIndex = stockCode.IndexOf("\"") + 1;
                        stockCode = stockCode.Substring(startIndex, stockCode.Length - startIndex);
                        endIndex = stockCode.IndexOf("\"");
                        stockLists += stockCode.Substring(0, endIndex) + "/space";
                        count++;
                        if (stockLists.Contains("<a"))
                        {
                            endIndex = stockLists.IndexOf("<a");
                            string firstPart = stockLists.Substring(0, endIndex);
                            stockLists = stockLists.Substring(endIndex + 2, stockLists.Length - (endIndex + 2));
                            startIndex = stockLists.IndexOf(">") + 1;
                            string secondPart = stockLists.Substring(startIndex, stockLists.Length - startIndex);
                            stockLists = firstPart + "\n" + secondPart;
                        }
                        if (stockLists.Contains("<span"))
                        {
                            endIndex = stockLists.IndexOf("<span");
                            string firstPart = stockLists.Substring(0, endIndex);
                            stockLists = stockLists.Substring(endIndex + 5, stockLists.Length - (endIndex + 5));
                            startIndex = stockLists.IndexOf(">") + 1;
                            string secondPart = stockLists.Substring(startIndex, stockLists.Length - startIndex);
                            stockLists = firstPart + "\n" + secondPart;
                        }
                        stockLists = stockLists.Replace("<li>", "");
                        stockLists = stockLists.Replace("\n", "");
                        stockLists = stockLists.Replace("</a>", "").Trim();
                        stockLists = stockLists.Replace("&#039;", "'");
                    }
                    stockLists = count + "  " + stockLists.Replace("/space", ", ");
                }

                // Following - Its name lists
                string followingLists = "";
                if (following != 0)
                {
                    startIndex = sourceCode.IndexOf("Following " + following + " members");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf("<ol>") + 4;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</ol");
                    string followingListsCode = sourceCode.Substring(0, endIndex);
                    while (followingListsCode.IndexOf("<li>") != -1)
                    {
                        startIndex = followingListsCode.IndexOf("<li>") + 4;
                        followingListsCode = followingListsCode.Substring(startIndex, followingListsCode.Length - startIndex);
                        endIndex = followingListsCode.IndexOf("</");
                        followingLists += followingListsCode.Substring(0, endIndex) + "/space";
                        if (followingLists.Contains("<a"))
                        {
                            endIndex = followingLists.IndexOf("<a");
                            string firstPart = followingLists.Substring(0, endIndex);
                            followingLists = followingLists.Substring(endIndex + 2, followingLists.Length - (endIndex + 2));
                            startIndex = followingLists.IndexOf(">") + 1;
                            string secondPart = followingLists.Substring(startIndex, followingLists.Length - startIndex);
                            followingLists = firstPart + "\n" + secondPart;
                        }
                        if (followingLists.Contains("<span"))
                        {
                            endIndex = followingLists.IndexOf("<span");
                            string firstPart = followingLists.Substring(0, endIndex);
                            followingLists = followingLists.Substring(endIndex + 5, followingLists.Length - (endIndex + 5));
                            startIndex = followingLists.IndexOf(">") + 1;
                            string secondPart = followingLists.Substring(startIndex, followingLists.Length - startIndex);
                            followingLists = firstPart + "\n" + secondPart;
                        }
                        followingLists = followingLists.Replace("<li>", "");
                        followingLists = followingLists.Replace("\n", "");
                        followingLists = followingLists.Replace(" ", "");
                        followingLists = followingLists.Replace("</a>", "").Trim();
                        followingLists = followingLists.Replace("&#039;", "'");
                    }
                    followingLists = followingLists.Replace("/space", ", ");
                }

                // Followers - Its name lists
                string followersLists = "";
                if (followers != 0)
                {
                    startIndex = sourceCode.IndexOf("Followed by " + followers + " members");
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    startIndex = sourceCode.IndexOf("<ol>") + 4;
                    sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                    endIndex = sourceCode.IndexOf("</ol");
                    string followersListsCode = sourceCode.Substring(0, endIndex);
                    while (followersListsCode.IndexOf("<li>") != -1)
                    {
                        startIndex = followersListsCode.IndexOf("<li>") + 4;
                        followersListsCode = followersListsCode.Substring(startIndex, followersListsCode.Length - startIndex);
                        endIndex = followersListsCode.IndexOf("</");
                        followersLists += followersListsCode.Substring(0, endIndex) + "/space";
                        if (followersLists.Contains("<a"))
                        {
                            endIndex = followersLists.IndexOf("<a");
                            string firstPart = followersLists.Substring(0, endIndex);
                            followersLists = followersLists.Substring(endIndex + 2, followersLists.Length - (endIndex + 2));
                            startIndex = followersLists.IndexOf(">") + 1;
                            string secondPart = followersLists.Substring(startIndex, followersLists.Length - startIndex);
                            followersLists = firstPart + "\n" + secondPart;
                        }
                        if (followersLists.Contains("<span"))
                        {
                            endIndex = followersLists.IndexOf("<span");
                            string firstPart = followersLists.Substring(0, endIndex);
                            followersLists = followersLists.Substring(endIndex + 5, followersLists.Length - (endIndex + 5));
                            startIndex = followersLists.IndexOf(">") + 1;
                            string secondPart = followersLists.Substring(startIndex, followersLists.Length - startIndex);
                            followersLists = firstPart + "\n" + secondPart;
                        }
                        followersLists = followersLists.Replace("<li>", "");
                        followersLists = followersLists.Replace("\n", "");
                        followersLists = followersLists.Replace(" ", "");
                        followersLists = followersLists.Replace("</a>", "").Trim();
                        followersLists = followersLists.Replace("&#039;", "'");
                    }
                    followersLists = followersLists.Replace("/space", ", ");
                }

                // Num of posts in a calendar month
                int numofPostsinaCalendarMonth = numOfPostsInAMonth(authorPostLink);


                if (!db.HotCopper_Authors.Any(f => f.Name == authorName))
                {
                    db.HotCopper_Authors.Add(new HotCopper_Authors
                    {
                        Name = authorName,
                        Num_of_Posts = postsTotalNum,
                        Likes_Received = likesReceived,
                        Followers = followers,
                        Followers_List = followersLists,
                        Following = following,
                        Following_List = followingLists,
                        Following_Stocks = stockLists,
                        Num_of_Posts_in_calendar_month = numofPostsinaCalendarMonth
                    });
                    db.SaveChanges();
                }
                else
                {
                    var existing = (from u in db.HotCopper_Authors
                                    where u.Name == authorName
                                    select u).FirstOrDefault();
                    if (existing != null)
                    {
                        if (existing.Num_of_Posts != postsTotalNum)
                            existing.Num_of_Posts = postsTotalNum;
                        if (existing.Likes_Received != likesReceived)
                            existing.Likes_Received = likesReceived;
                        if (existing.Followers != followers)
                            existing.Followers = followers;
                        if (existing.Following != following)
                            existing.Following = following;
                        if (existing.Followers_List != followersLists)
                            existing.Followers_List = followersLists;
                        if (existing.Following_List != followingLists)
                            existing.Following_List = followingLists;
                        if (existing.Num_of_Posts_in_calendar_month != numofPostsinaCalendarMonth)
                            existing.Num_of_Posts_in_calendar_month = numofPostsinaCalendarMonth;
                        if (existing.Following_Stocks != stockLists)
                            existing.Following_Stocks = stockLists;
                        db.SaveChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);  // May not display in the form
            }
        }

        public static int numOfPostsInAMonth(string authorPostLink)
        {
            try
            {
                string sourceCode = WorkerClasses.getSourceCode(authorPostLink);
                if (sourceCode == "invalid") throw new UriFormatException();

                int startIndex = 0; int endIndex = 0; int count = 0;
                if (sourceCode.IndexOf("Next</a>") == -1)
                {
                    while (sourceCode.IndexOf("listblock time") != -1)
                    {
                        startIndex = sourceCode.IndexOf("listblock time") + 14;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        startIndex = sourceCode.IndexOf(">") + 1;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        startIndex = sourceCode.IndexOf(">") + 1;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        endIndex = sourceCode.IndexOf("</");
                        string timeStr = sourceCode.Substring(0, endIndex);
                        DateTime postTime = Convert.ToDateTime(timeStr);
                        int postMonth = postTime.Month;
                        int nowMonth = DateTime.Now.Month;
                        if (postMonth == nowMonth) count++;
                    }
                }
                else
                {
                    while (sourceCode.IndexOf("Next</a>") != -1)
                    {
                        while (sourceCode.IndexOf("listblock time") != -1)
                        {
                            startIndex = sourceCode.IndexOf("listblock time") + 14;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf(">") + 1;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            startIndex = sourceCode.IndexOf(">") + 1;
                            sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                            endIndex = sourceCode.IndexOf("</");
                            string timeStr = sourceCode.Substring(0, endIndex);
                            DateTime postTime = Convert.ToDateTime(timeStr);
                            int postMonth = postTime.Month;
                            int nowMonth = DateTime.Now.Month;
                            if (postMonth == nowMonth) count++;
                        }
                        endIndex = sourceCode.IndexOf("Next</a>");
                        sourceCode = sourceCode.Substring(0, endIndex);
                        startIndex = sourceCode.LastIndexOf("</a>") + 4;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        startIndex = sourceCode.IndexOf("a href") + 1;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        startIndex = sourceCode.IndexOf("\"") + 1;
                        sourceCode = sourceCode.Substring(startIndex, sourceCode.Length - startIndex);
                        endIndex = sourceCode.IndexOf("\"");
                        string nextPageLink = "http://hotcopper.com.au/" + sourceCode.Substring(0, endIndex).Replace("&amp;", "&");
                        if (nextPageLink.Contains("disabled")) break;
                        else
                        {
                            sourceCode = WorkerClasses.getSourceCode(nextPageLink);
                            if (sourceCode == "invalid") throw new UriFormatException();
                        }
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
