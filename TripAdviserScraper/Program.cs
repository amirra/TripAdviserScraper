using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Web;
using System.Text.RegularExpressions;
using TripAdviserScraper.Models;

namespace TripAdviserScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new TripAdviserEntities())
            {
                var extract_images = false;
                var extract_comments = true;

                var addresses = new string[] { "/Tourism-g295424-Dubai_Emirate_of_Dubai-Vacations.html" };

                var attractions_address = new List<Address>();

                foreach (var item in addresses)
                {
                    var id = item.Split('-')[1];
                    var link_default = item.Split('-')[2];
                    var urls = new string[] { "https://www.tripadvisor.com/Restaurants-" + id + "-" + link_default, "https://www.tripadvisor.com/Attractions-" + id + "-" + link_default };//, };

                    foreach (var url in urls)
                    {
                        var html = string.Format(url);
                        HtmlWeb web = new HtmlWeb();
                        var htmlDoc_main = web.Load(html);

                        var last_number = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pageNumbers"));
                        var last_page_number = !last_number.Any() ? 1 : int.Parse(last_number.First().ChildNodes.Where(c => c.Name.Equals("a")).Last().Attributes["data-page-number"].Value);


                        for (int i = 0; i < last_page_number; i++)
                        {
                            if (i == 0)
                            {
                                if (url.Contains("Attractions"))
                                {
                                    var list_items = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_details"));

                                    foreach (var list_item in list_items)
                                    {
                                        attractions_address.Add(new Address
                                        {
                                            LinkAddress = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_info")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_title")).First().ChildNodes.Where(c => c.Name.Equals("a")).Last().Attributes["href"].Value
                                        });
                                    }
                                }
                                else
                                {
                                    var list_items = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing"));

                                    foreach (var list_item in list_items)
                                    {
                                        var link = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("ui_columns")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("shortSellDetails")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("title")).First().ChildNodes.Where(x => x.Name.Equals("a")).First().Attributes["href"].Value;
                                        attractions_address.Add(new Address { LinkAddress = link });
                                    }
                                }
                            }
                            else
                            {
                                if (url.Contains("Attractions"))
                                {
                                    htmlDoc_main = web.Load("https://www.tripadvisor.com/Attractions-" + id + "-Activities-oa" + i * 30 + "-" + link_default);

                                    var list_items = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_details"));

                                    foreach (var list_item in list_items)
                                    {
                                        attractions_address.Add(new Address { LinkAddress = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_info")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing_title")).First().ChildNodes.Where(c => c.Name.Equals("a")).Last().Attributes["href"].Value });
                                    }
                                }
                                else
                                {
                                    htmlDoc_main = web.Load("https://www.tripadvisor.com/Restaurants-" + id + "-oa" + i * 30 + "-" + link_default);

                                    var list_items = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("listing"));

                                    foreach (var list_item in list_items)
                                    {
                                        var link = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("ui_columns")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("shortSellDetails")).First().ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("title")).First().ChildNodes.Where(x => x.Name.Equals("a")).First().Attributes["href"].Value;
                                        attractions_address.Add(new Address { LinkAddress = link });
                                    }
                                }


                            }
                        }

                        foreach (var photoaddress in attractions_address)
                        {
                            if (extract_comments)
                            {
                                try
                                {
                                    var id_item = photoaddress.LinkAddress.Split('-')[2];
                                    var location_item = ctx.Locations.First(x => x.LocationRef.Equals(id_item));

                                    var link = "https://www.tripadvisor.com" + photoaddress.LinkAddress;
                                    var htmlDoc = web.Load(link);

                                    var last_number_comment = htmlDoc.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pageNum last"));
                                    var last_page_number_comment = !last_number_comment.Any() ? 1 : int.Parse(last_number_comment.First().InnerText);


                                    for (int i = 0; i < last_page_number_comment; i++)
                                    {
                                        if (i == 0)
                                        {
                                            var list_items = htmlDoc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("innerBubble"));

                                            foreach (var list_item in list_items)
                                            {
                                                var item_info = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("wrap")).First();
                                                var item_info_title = item_info.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("quote")).First().InnerText;
                                                var item_info_entry = item_info.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("prw_rup prw_reviews_text_summary_hsx")).First().InnerText;

                                                var comment = new LocationComment { Title_Original = item_info_title, Description_Original = item_info_entry };

                                                location_item.LocationComments.Add(comment);
                                            }
                                        }
                                        else
                                        {
                                            htmlDoc_main = web.Load(link.Replace("Reviews", "Reviews-or" + 10 * i));
                                            var list_items = htmlDoc_main.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("innerBubble"));

                                            foreach (var list_item in list_items)
                                            {
                                                var item_info = list_item.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("wrap")).First();
                                                var item_info_title = item_info.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("quote")).First().InnerText;
                                                var item_info_entry = item_info.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("prw_rup prw_reviews_text_summary_hsx")).First().InnerText;

                                                var comment = new LocationComment { Title_Original = item_info_title, Description_Original = item_info_entry };
                                                location_item.LocationComments.Add(comment);
                                            }
                                        }
                                    }

                                    ctx.SaveChanges();
                                }
                                catch (Exception)
                                {


                                }
                            }

                            if (extract_images)
                            {
                                var images = new List<LocationImage>();
                                var address = photoaddress.LinkAddress.Split('-')[2];
                                var htmlDoc = web.Load("https://www.tripadvisor.com/LocationPhotoAlbum?detail=" + address);
                                var list_items = htmlDoc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("tinyThumb"));
                                var location = new Location();

                                try
                                {
                                    var htmlDoc_info = web.Load("https://www.tripadvisor.com" + photoaddress.LinkAddress);
                                    var ranking = htmlDoc_info.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("header_popularity"));
                                    var rank = ranking.Any() ? int.Parse(ranking.First().ChildNodes.Where(d => d.Name.Equals("b")).First().InnerText.Replace("#", "")) : 0;
                                    var title = htmlDoc_info.DocumentNode.Descendants("h1").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("heading_title")).First().InnerText;
                                    var province = item.Split('-')[2].Split('_')[0];

                                    location.LocationRef = address;
                                    location.Rank = rank;
                                    location.Title = title;
                                    location.CityTitle = province;
                                }
                                catch (Exception)
                                {

                                }

                                Directory.CreateDirectory(string.Format("f:\\TripAdviser\\{0}", address));

                                foreach (var img in list_items)
                                {
                                    try
                                    {
                                        var image = img.ChildNodes.Where(d => d.Name.Equals("a")).First().ChildNodes.Where(x => x.Name.Equals("img")).First().Attributes["src"].Value.Replace("photo-t", "photo-o");
                                        var name = Guid.NewGuid() + Path.GetExtension(image);
                                        var webClient = new WebClient();
                                        webClient.DownloadFile(image, string.Format("f:\\TripAdviser\\{0}\\{1}", address, name));
                                        location.LocationImages.Add(new LocationImage { Name = name, Address = image });
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                ctx.Locations.Add(location);
                                ctx.SaveChanges();
                            }
                        }
                    }

                    attractions_address.Clear();
                }
            }
        }
    }
}
