﻿using NewsApp.Exceptions;
using NewsApp.Models;
using NewsApp.Repositories.Interfaces;
using NewsApp.Services.Interfaces;
using Newtonsoft.Json.Linq;
using Sprache;
using static NewsApp.Models.Enum;
using NewsApp.DTOs;
using NewsApp.Mappers;
using Microsoft.Extensions.Caching.Memory;


namespace NewsApp.Services.Classes
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IRepository<string, Category, string> _categoryRepository;
        private readonly IArticleCategoryRepository _articlecategoryRepository;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        private const string MinNewsIdCacheKey = "MinNewsId";

        public ArticleService(IArticleRepository articleRepository, 
            HttpClient httpClient, 
            IRepository<string, Category, string> categoryRepository, 
            IArticleCategoryRepository articlecategoryRepository,
             IMemoryCache cache
            )
        {
            _articleRepository = articleRepository;
            _categoryRepository = categoryRepository;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://m.inshorts.com/en/read");
            _articlecategoryRepository = articlecategoryRepository;
            _cache = cache;
        }

        public async Task<AdminArticleReturnDTO> ChangeArticleStatus(string articleId, ArticleStatus articleStatus)
        {
            var existingArticle = await _articleRepository.Get("ArticleID", articleId);
            if(existingArticle == null)
            {
                throw new ItemNotFoundException();
            }
            existingArticle.Status = articleStatus;

            var result = await _articleRepository.Update(existingArticle, existingArticle.ArticleID.ToString());
            if(result == null)
            {
                throw new UnableToUpdateItemException();
            }
            return ArticleMapper.MapAdminArticleReturnDTO(result);             
        }

        public async Task FetchAndSaveArticlesAsync()
        {
            var minNewsId = _cache.Get<string>(MinNewsIdCacheKey);
            string url = minNewsId == null
                ? "https://m.inshorts.com/api/in/en/news?category=top_stories&max_limit=10&include_card_data=true"
                : $"https://m.inshorts.com/api/in/en/news?category=top_stories&max_limit=10&include_card_data=true&news_offset={minNewsId}";

            //_httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            //_httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,en-GB;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            _httpClient.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.1.527841905.1721632649; _tenant=ENGLISH; mp_a99a1037068944b00a0cc8ee56c94e8c_mixpanel=%7B%22distinct_id%22%3A%20%22%24device%3A190d94d711c3d5-09f3dfb386b4b-4c657b58-1fa400-190d94d711c3d6%22%2C%22%24device_id%22%3A%20%22190d94d711c3d5-09f3dfb386b4b-4c657b58-1fa400-190d94d711c3d6%22%2C%22%24search_engine%22%3A%20%22bing%22%2C%22%24initial_referrer%22%3A%20%22https%3A%2F%2Fwww.bing.com%2F%22%2C%22%24initial_referring_domain%22%3A%20%22www.bing.com%22%2C%22__mps%22%3A%20%7B%7D%2C%22__mpso%22%3A%20%7B%22%24initial_referrer%22%3A%20%22https%3A%2F%2Fwww.bing.com%2F%22%2C%22%24initial_referring_domain%22%3A%20%22www.bing.com%22%7D%2C%22__mpus%22%3A%20%7B%7D%2C%22__mpa%22%3A%20%7B%7D%2C%22__mpu%22%3A%20%7B%7D%2C%22__mpr%22%3A%20%5B%5D%2C%22__mpap%22%3A%20%5B%5D%7D; _ga_L7P7D50590=GS1.1.1722022582.17.1.1722024549.60.0.0");
            _httpClient.DefaultRequestHeaders.Add("Priority", "u=0, i");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Microsoft Edge\";v=\"126\"");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Android\"");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Mobile Safari/537.36 Edg/126.0.0.0");

            try
            {
                var response = await _httpClient.GetStringAsync(url);


                var json = JObject.Parse(response);
                Console.WriteLine(json);
                var newsList = json["data"]?["news_list"]?.ToArray();
                var newMinNewsId = json["data"]?["min_news_id"]?.ToString();

                if (!string.IsNullOrEmpty(newMinNewsId))
                {
                    _cache.Set(MinNewsIdCacheKey, newMinNewsId);
                }

                if (newsList == null || newsList.Length == 0)
                {
                    return;
                }

                foreach (var newsItem in newsList)
                {
                    var newsObj = newsItem["news_obj"];
                    if (newsObj == null) continue;
                    var version = (int)newsItem["version"];

                    var hashId = (string)newsObj["hash_id"];
                    var oldHashId = (string)newsObj["old_hash_id"];

                    var categories = newsObj["category_names"];

                    var article = new Article
                    {
                        OldHashID = oldHashId,
                        HashID = hashId,
                        Title = (string)newsObj["title"],
                        Content = (string)newsObj["content"],
                        Summary = (string)newsObj["bottom_headline"],
                        AddedAt = DateTime.UtcNow,
                        OriginURL = (string)newsObj["source_url"],
                        ImgURL = (string)newsObj["image_url"],
                        CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds((long)newsObj["created_at"]).UtcDateTime,
                        ImpScore = (decimal)newsObj["impressive_score"],
                        SaveCount = 0,
                        Status = ArticleStatus.Pending
                    };

                    var existingArticle = version == 0
                        ? await _articleRepository.Get("HashID", hashId)
                        : await _articleRepository.Get("OldHashID", oldHashId);

                    if (existingArticle == null)
                    {
                        var result = await _articleRepository.Add(article);
                        if (result.ArticleID == null)
                        {
                            throw new UnableToAddItemException();
                        }

                        foreach (var category in categories)
                        {
                            string category1 = (string)category;
                            int CategoryID;

                            var validCategory = await _categoryRepository.Get("Description", category1);
                            if (validCategory == null)
                            {
                                Category newCategory = new Category()
                                {
                                    Description = category1,
                                    Name = char.ToUpper(category1[0]) + category1.Substring(1).ToLower()
                                };
                                var result1 = await _categoryRepository.Add(newCategory);
                                if (result1.CategoryID == null)
                                {
                                    throw new UnableToAddItemException();
                                }
                                CategoryID = result1.CategoryID;
                            }
                            else
                            {
                                CategoryID = validCategory.CategoryID;
                            }

                            ArticleCategory articleCategory = new ArticleCategory()
                            {
                                ArticleID = result.ArticleID,
                                CategoryID = CategoryID,
                            };

                            var articleCategoryResult = await _articlecategoryRepository.Add(articleCategory);
                            if (articleCategoryResult.ArticleID == null)
                            {
                                throw new UnableToAddItemException();
                            }
                        }
                        //Add Top Stories category to every article fetched
                        ArticleCategory articleCategory1 = new ArticleCategory()
                        {
                            ArticleID = result.ArticleID,
                            CategoryID = 28,
                        };
                        var articleCategoryResult1 = await _articlecategoryRepository.Add(articleCategory1);
                        if (articleCategoryResult1.ArticleID == null)
                        {
                            throw new UnableToAddItemException();
                        }
                    }
                    else
                    {

                        if (existingArticle.HashID != hashId)
                        {
                            existingArticle.Title = article.Title;
                            existingArticle.Content = article.Content;
                            existingArticle.Summary = article.Summary;
                            existingArticle.ImgURL = article.ImgURL;
                            existingArticle.AddedAt = article.AddedAt;
                            existingArticle.OriginURL = article.OriginURL;
                            existingArticle.CreatedAt = article.CreatedAt;
                            existingArticle.ImpScore = article.ImpScore;

                            existingArticle.HashID = hashId;
                            existingArticle.OldHashID = oldHashId;
                            existingArticle.Status = ArticleStatus.Pending; //Edited article change status to pending

                            await _articleRepository.Update(existingArticle, existingArticle.ArticleID.ToString());
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine($"Inner exception: {innerEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

        }

        public async Task FetchAndSaveCategoryArticlesAsync()
        {
            var categories = await _categoryRepository.GetAll("",""); 

            foreach (var category in categories)
            {
                var pageNumber = _cache.TryGetValue($"CategoryPage_{category.CategoryID}", out int cachedPageNumber) ? cachedPageNumber : 1;
                await FetchAndSaveArticlesByCategoryAsync(category, pageNumber);
            }
        }

        private async Task FetchAndSaveArticlesByCategoryAsync(Category category, int pageNumber)
        {
            string url = $"https://m.inshorts.com/api/en/search/trending_topics/{category.Description}?page={pageNumber}&type={category.Type}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,en-GB;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            _httpClient.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.1.527841905.1721632649; _tenant=ENGLISH; mp_a99a1037068944b00a0cc8ee56c94e8c_mixpanel=%7B%22distinct_id%22%3A%20%22%24device%3A190d94d711c3d5-09f3dfb386b4b-4c657b58-1fa400-190d94d711c3d6%22%2C%22%24device_id%22%3A%20%22190d94d711c3d5-09f3dfb386b4b-4c657b58-1fa400-190d94d711c3d6%22%2C%22%24search_engine%22%3A%20%22bing%22%2C%22%24initial_referrer%22%3A%20%22https%3A%2F%2Fwww.bing.com%2F%22%2C%22%24initial_referring_domain%22%3A%20%22www.bing.com%22%2C%22__mps%22%3A%20%7B%7D%2C%22__mpso%22%3A%20%7B%22%24initial_referrer%22%3A%20%22https%3A%2F%2Fwww.bing.com%2F%22%2C%22%24initial_referring_domain%22%3A%20%22www.bing.com%22%7D%2C%22__mpus%22%3A%20%7B%7D%2C%22__mpa%22%3A%20%7B%7D%2C%22__mpu%22%3A%20%7B%7D%2C%22__mpr%22%3A%20%5B%5D%2C%22__mpap%22%3A%20%5B%5D%7D; _ga_L7P7D50590=GS1.1.1722022582.17.1.1722024549.60.0.0");
            _httpClient.DefaultRequestHeaders.Add("Priority", "u=0, i");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Microsoft Edge\";v=\"126\"");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Android\"");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Mobile Safari/537.36 Edg/126.0.0.0");
            _httpClient.DefaultRequestHeaders.Clear();                                         
            try
            {
                var response = await _httpClient.GetStringAsync(url);

                var json = JObject.Parse(response);
                Console.WriteLine(json);
                var newsList = json["data"]?["news_list"]?.ToArray();

                if (newsList == null || newsList.Length == 0)
                {
                    return;
                }

                foreach (var newsItem in newsList)
                {
                    var newsObj = newsItem["news_obj"];
                    if (newsObj == null) continue;
                    var version = (int)newsItem["version"];

                    var hashId = (string)newsObj["hash_id"];
                    var oldHashId = (string)newsObj["old_hash_id"];

                    var categories = newsObj["category_names"];

                    var article = new Article
                    {
                        OldHashID = oldHashId,
                        HashID = hashId,
                        Title = (string)newsObj["title"],
                        Content = (string)newsObj["content"],
                        Summary = (string)newsObj["bottom_headline"],
                        AddedAt = DateTime.UtcNow,
                        OriginURL = (string)newsObj["source_url"],
                        ImgURL = (string)newsObj["image_url"],
                        CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds((long)newsObj["created_at"]).UtcDateTime,
                        ImpScore = (decimal)newsObj["impressive_score"],
                        SaveCount = 0,
                        Status = ArticleStatus.Pending
                    };

                    var existingArticle = version == 0
                        ? await _articleRepository.Get("HashID", hashId)
                        : await _articleRepository.Get("OldHashID", oldHashId);

                    if (existingArticle == null)
                    {
                        var result = await _articleRepository.Add(article);
                        if (result.ArticleID == 0)
                        {
                            throw new UnableToAddItemException();
                        }

                        foreach (var categoryName in categories)
                        {
                            string category1 = (string)categoryName;
                            int CategoryID;

                            var validCategory = await _categoryRepository.Get("Description", category1);
                            if (validCategory == null)
                            {
                                Category newCategory = new Category()
                                {
                                    Description = category1,
                                    Name = char.ToUpper(category1[0]) + category1.Substring(1).ToLower(),
                                    Type = "CUSTOM_CATEGORY"
                                };
                                var result1 = await _categoryRepository.Add(newCategory);
                                if (result1.CategoryID == 0)
                                {
                                    throw new UnableToAddItemException();
                                }
                                CategoryID = result1.CategoryID;
                            }
                            else
                            {
                                CategoryID = validCategory.CategoryID;
                            }

                            ArticleCategory articleCategory = new ArticleCategory()
                            {
                                ArticleID = result.ArticleID,
                                CategoryID = CategoryID,

                            };

                            var articleCategoryResult = await _articlecategoryRepository.Add(articleCategory);
                            if (articleCategoryResult.ArticleID == 0)
                            {
                                throw new UnableToAddItemException();
                            }
                        }
                     
                    }
                    else
                    {
                        if (existingArticle.HashID != hashId)
                        {
                            existingArticle.Title = article.Title;
                            existingArticle.Content = article.Content;
                            existingArticle.Summary = article.Summary;
                            existingArticle.ImgURL = article.ImgURL;
                            existingArticle.AddedAt = article.AddedAt;
                            existingArticle.OriginURL = article.OriginURL;
                            existingArticle.CreatedAt = article.CreatedAt;
                            existingArticle.ImpScore = article.ImpScore;

                            existingArticle.HashID = hashId;
                            existingArticle.OldHashID = oldHashId;
                            existingArticle.Status = ArticleStatus.Pending; // Edited article change status to pending

                            await _articleRepository.Update(existingArticle, existingArticle.ArticleID.ToString());
                        }
                    }
                }

                // Increment page number and store in cache
                _cache.Set($"CategoryPage_{category.CategoryID}", pageNumber + 1);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine($"Inner exception: {innerEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<AdminArticlePaginatedReturnDTO> GetPaginatedArticlesAsync(int pageNumber, int pageSize, string status, int categoryID)
        {

            if (!System.Enum.TryParse(status, true, out ArticleStatus articleStatus))
            {
                throw new ArgumentException("Invalid status value");
            }

            var allArticles = await _articleRepository.GetAllByStatusAndCategoryAsync(articleStatus, categoryID);


            if (allArticles == null || allArticles.Count() == 0)
            {
                throw new NoAvailableItemException();
            }


            int skip = (pageNumber - 1) * pageSize;

            var paginatedArticles = allArticles
               .OrderByDescending(article => article.CreatedAt)
               .Skip(skip)
               .Take(pageSize);

            var totalPages = (int)Math.Ceiling(allArticles.Count() / (double)pageSize);

            return new AdminArticlePaginatedReturnDTO{
                Articles=paginatedArticles.Select(x => ArticleMapper.MapAdminArticleReturnDTO(x)),
                totalpages=totalPages
            };
        }

        public async Task<AdminArticlePaginatedReturnDTO> GetPaginatedArticlesForUserAsync(int pageNumber, int pageSize, int categoryID)
        {

            var allArticles = await _articleRepository.GetAllApprcvedEditedArticlesAndCategoryAsync(categoryID);


            if (allArticles == null || allArticles.Count() == 0)
            {
                throw new NoAvailableItemException();
            }


            int skip = (pageNumber - 1) * pageSize;

            var paginatedArticles = allArticles
               .OrderByDescending(article => article.CreatedAt)
               .Skip(skip)
               .Take(pageSize);

            var totalPages = (int)Math.Ceiling(allArticles.Count() / (double)pageSize);

            return new AdminArticlePaginatedReturnDTO
            {
                Articles = paginatedArticles.Select(x => ArticleMapper.MapAdminArticleReturnDTO(x)),
                totalpages = totalPages
            };
        }


        public async Task<AdminArticleReturnDTO> EditArticleData(AdminArticleReturnDTO adminArticleReturnDTO)
        {
            var article = await _articleRepository.Get("ArticleID", adminArticleReturnDTO.ArticleID.ToString());

            if (article == null)
            {
                throw new ItemNotFoundException();
            }

            article.Title = adminArticleReturnDTO.Title;
            article.Content = adminArticleReturnDTO.Content;
            article.ImgURL = adminArticleReturnDTO.ImgURL;
            article.Summary = adminArticleReturnDTO.Summary;
            article.OriginURL =adminArticleReturnDTO.OriginURL;
            article.Status = ArticleStatus.Edited;

            var result = await _articleRepository.Update(article, article.ArticleID.ToString());
            if(result != null) { 
                return ArticleMapper.MapAdminArticleReturnDTO(result);
            }

            throw new UnableToUpdateItemException();
        }



    }
}
