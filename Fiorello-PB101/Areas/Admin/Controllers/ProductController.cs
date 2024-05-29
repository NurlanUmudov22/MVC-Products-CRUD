using Fiorello_PB101.Data;
using Fiorello_PB101.Extensions;
using Fiorello_PB101.Helpers;
using Fiorello_PB101.Models;
using Fiorello_PB101.Services;
using Fiorello_PB101.Services.Interfaces;
using Fiorello_PB101.ViewModels.Blog;
using Fiorello_PB101.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fiorello_PB101.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public ProductController(IProductService productService, 
                                 ICategoryService categoryService,
                                 IWebHostEnvironment env,
                                 AppDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _env = env;
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _productService.GetAllPaginateAsync(page, 4);

            var mappedDatas = _productService.GetMappedDatas(products);

            int totalPage = await GetPageCountAsync(4);

            Paginate<ProductVM> paginateDatas = new(mappedDatas, totalPage, page);

            return View(paginateDatas);
        }

        private async Task<int> GetPageCountAsync(int take)
        {
            int productCount = await _productService.GetCountAsync();

            return (int)Math.Ceiling((decimal)productCount / take);
        }




        [HttpGet]
        public  async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int) id);

            if (existProduct == null) return NotFound();


            List<ProductImageVM> images = new();

            foreach (var item in existProduct.ProductImages)
            {
                images.Add(new ProductImageVM
                {
                    Image = item.Name,
                    IsMain = item.IsMain
                });
            }



            ProductDetailVM response = new()
            {
                Name = existProduct.Name,
                Description = existProduct.Description,
                Category = existProduct.Category.Name,
                Price = existProduct.Price,
                Images = images

            };


            return View(response);
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = await _categoryService.GetAllSelectedAsync();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM request)
        {
            ViewBag.categories = await _categoryService.GetAllSelectedAsync();


            if (!ModelState.IsValid)
            {
                return View();
    
            }

            foreach (var item in request.Images)
            {
                if (!item.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Images", "Image can accept only image format");
                    return View();
                }

                if (!(item.CheckFileSize(500)))
                {
                    ModelState.AddModelError("Images", "Image size must be max 500 KB");
                    return View();
                }
            }


            List<ProductImage> images = new();

            foreach (var item in request.Images)
            {
                string fileName = $"{Guid.NewGuid()}-{item.FileName}";                 
                string path = _env.GeneratedFilePath("img", fileName);
              
                await item.SaveFileToLocalAsync(path);

                images.Add(new ProductImage { Name = fileName });

            }


            images.FirstOrDefault().IsMain = true;


            Product product = new()
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Price = decimal.Parse(request.Price.Replace(".",",")),
                ProductImages = images
            };

            await _productService.CreateAsync(product);


            return RedirectToAction("Index");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int)id);

            if (existProduct == null) return NotFound();

                foreach (var item in existProduct.ProductImages)
            {
                string path = _env.GeneratedFilePath("img", item.Name);
                path.DeleteFileFromLocal();
            }

            await _productService.DeleteAsync(existProduct);

            return RedirectToAction("Index");

        }





        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {

            ViewBag.categories = await _categoryService.GetAllSelectedAsync();

            if (id == null) return BadRequest();

            var product = await _productService.GetByIdAsync(id);

            if (product == null) return NotFound();

            return View(new ProductEditVM
            {
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Price = product.Price.ToString(),
                NewImage = (IFormFile)product.ProductImages

            });

        }




        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int? id, ProductEditVM request)
        {

            ViewBag.categories = await _categoryService.GetAllSelectedAsync();

            //if (!ModelState.IsValid)
            //{
            //    return View();
            //}


            if (id == null) return BadRequest();

            var product = await _productService.GetByIdAsync(id);

            if (product == null) return NotFound();

            //if (request.NewImage == null) return RedirectToAction("Index");

            if (await _productService.ExistExceptByIdAsync((int)id, request.Name))
            {
                ModelState.AddModelError("Title", "This category already exist");
                return View();
            }

            if (!request.NewImage.CheckFileType("image/"))
            {
                ModelState.AddModelError("NewImage", "Image can accept only image format");
                request.NewImage = (IFormFile)product.ProductImages;
                return View(request);
            }

            if (!(request.NewImage.CheckFileSize(500)))
            {
                ModelState.AddModelError("NewImage", "Image size must be max 200 KB");
                request.NewImage = (IFormFile)product.ProductImages;
                return View(request);
            }

            if (product.Name == request.Name)
            {
                return RedirectToAction("Index");
            }


            //string oldPath = _env.GeneratedFilePath("img", request.Image);

            //oldPath.DeleteFileFromLocal();

            string fileName = Guid.NewGuid().ToString() + "-" + request.NewImage.FileName;

            string newPath = _env.GeneratedFilePath("img", fileName);

            await request.NewImage.SaveFileToLocalAsync(newPath);

            //product.ProductImages = fileName;


            product.Name = request.Name;

            product.Description = request.Description;

            product.CategoryId = request.CategoryId;

            product.Price = decimal.Parse(request.Price);

           


            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        }
    }
