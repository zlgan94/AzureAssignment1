using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Linq;
using Grocery.WebApp.Data;
using Grocery.WebApp.Models;
using Grocery.WebApp.Areas.Admin.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Grocery.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductsController : Controller
    {
        private const string BlobContainerNAME = "myimages";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly UserManager<MyIdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            ApplicationDbContext context,
            ILogger<ProductsController> logger,
            UserManager<MyIdentityUser> userManager,
            IConfiguration config,
            IWebHostEnvironment environment
            )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _config = config;
            _environment = environment;
        }

        // GET: ProductsController
        public ActionResult Index()
        {
            // LAMBDA Version to extract ALL PRODUCTS using Eager Loading
            //var products = _context.Products
            //                       .Include(p => p.CreatedByUser)
            //                       .Include(p => p.UpdatedByUser);
            //List<ProductViewModel> viewmodels = new List<ProductViewModel>();
            //foreach(var p in products)
            //{
            //    viewmodels.Add(new ProductViewModel
            //    {
            //        ProductID = p.ProductID,
            //        ProductName = p.ProductName,
            //        SellingPricePerUnit = p.SellingPricePerUnit,
            //        Quantity = p.Quantity,
            //        Image = p.Image,

            //        CreatedByUser = p.CreatedByUser,
            //        CreatedByUserId = p.CreatedByUserId,
            //        UpdatedByUser = p.UpdatedByUser,
            //        UpdatedByUserId = p.UpdatedByUserId
            //    });
            //}

            /*
            var connString = _config.GetValue<string>("ConnectionStrings:AzureConnection");
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("pub");
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("p033tw9j.jpg");

            using (MemoryStream ms = new MemoryStream())
            {
                cloudBlockBlob.DownloadToStream(ms);
                return File(ms.ToArray(), "image/jpeg");
            }
            */

            // LINQ Version to extract ALL PRODUCTS using Eager Loading
            //     AND project the data into the ViewModel (called Model Binding)
            var productViewModels = (from p in _context.Products
                                             .Include(p => p.CreatedByUser)
                                             .Include(p => p.UpdatedByUser)
                                   select new ProductViewModel
                                   {
                                       ProductID = p.ProductID,
                                       ProductName = p.ProductName,
                                       SellingPricePerUnit = p.SellingPricePerUnit,
                                       Quantity = p.Quantity,
                                       Image = p.Image,

                                       LastUpdatedOn = p.LastUpdatedOn,
                                       CreatedByUser = p.CreatedByUser,
                                       CreatedByUserId = p.CreatedByUserId,
                                       UpdatedByUser = p.UpdatedByUser,
                                       UpdatedByUserId = p.UpdatedByUserId
                                   })
                                   .ToList();
            return View(productViewModels);
        }

        // GET: ProductsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductsController/Create
        public ActionResult Create()
        {
            ProductViewModel productViewModel = new ProductViewModel(); 
            return View(productViewModel);
        }

        #region Helper Methods

        private async Task<string> fSaveToBlobStorage(string blobName, string filePath)
        {
            var storageConn
                = _config.GetValue<string>("ConnectionStrings:AzureConnection");

            // Get a reference to a Container
            BlobContainerClient blobContainerClient
                = new BlobContainerClient(storageConn, BlobContainerNAME);

            // Create the container if it does not exist - granting PUBLIC access.
            await blobContainerClient.CreateIfNotExistsAsync(
                Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            // Upload the file
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            //await blobClient.UploadAsync(filePath,overwrite:true);
            await blobClient.UploadAsync(filePath, new BlobHttpHeaders { ContentType = "image/jpeg" });

            // Return the URL of the file on successful upload.
            return blobClient.Uri.AbsoluteUri;
        }

        #endregion

        // POST: ProductsController/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind("ProductName,Quantity,SellingPricePerUnit")] ProductViewModel productViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("Create", "User not found.  Please log back in!");
            }

            if (!ModelState.IsValid)
            {
                return View(productViewModel);
            }

            Product newProduct = new Product()
            {
                //ProductID = new Guid(),
                ProductID = Guid.NewGuid(),
                ProductName = productViewModel.ProductName,
                SellingPricePerUnit = productViewModel.SellingPricePerUnit,
                Quantity = productViewModel.Quantity,

                LastUpdatedOn = DateTime.Now,
                CreatedByUserId = user.Id
            };

            /*
            // check if file has attached while submitting the Form
            if (Request.Form.Files.Count >= 1)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                // IFormFile file = productViewModel.ImageFile;

                // copy the file uploaded using the MemoryStream - into the Product.Image
                using(var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    newProduct.Image = dataStream.ToArray();
                }
            }
            */

            //for image posting to azure blob container
            IFormFile file = Request.Form.Files.FirstOrDefault();

            //var azureName = newProduct.ProductID.ToString() + ".jpg"; //this is to force it to be image if never set blobcontent type during uploadasync in helper method above, need to add .jpg in index.cshtml too, in the url path
            var azureName = newProduct.ProductID.ToString();
            // Check if file was uploaded, and is not an empty file.
            if (file != null || file.Length > 0)
            {

                // Save the uploaded file on a temporary "UploadedImages" folder in wwwroot.
                var filepath = Path.Combine(_environment.WebRootPath, "UploadedImages", file.FileName);
                //var filepath = Path.Combine(_environment.WebRootPath, "UploadedImages", newProduct.ProductID.ToString());
                using (var stream = System.IO.File.Create(filepath))
                {
                    file.CopyToAsync(stream).Wait();
                }

                // Upload the image to the Blob Container
                //string imgBlobUri = await this.fSaveToBlobStorage(file.FileName, filepath);
                string imgBlobUri = await this.fSaveToBlobStorage(azureName, filepath);

                // Delete the uploaded image file from the temporary folder, as not needed any more.
                System.IO.File.Delete(filepath);


                //var imgLink = $"<a href='{imgBlobUri}' target='_blank'>{imgBlobUri}</a>";
                //ViewBag.StatusType = "success";
                //ViewBag.StatusMessage
                //    = $"Saved file successfully to:<br />{imgLink}";
            }

            // update the database
            try
            {
                _context.Products.Add(newProduct);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch(System.Exception exp)
            {
                ModelState.AddModelError("Create", "Unable to update the Database. Contact Admin!");
                _logger.LogError($"Create Product failed: {exp.Message}");
                return View(productViewModel);
            }
        }

        // GET: ProductsController/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            // Get the data from the database
            Product productToEdit = await _context.Products.FindAsync(id);
            if(productToEdit == null)
            {
                return NotFound();
            }

            // Initialize the Viewmodel
            ProductViewModel productViewModel = new ProductViewModel()
            {
                ProductID = productToEdit.ProductID,
                ProductName = productToEdit.ProductName,
                SellingPricePerUnit = productToEdit.SellingPricePerUnit,
                Quantity = productToEdit.Quantity,
                Image = productToEdit.Image,
                
                LastUpdatedOn = productToEdit.LastUpdatedOn,
                CreatedByUserId = productToEdit.CreatedByUserId,
                CreatedByUser = productToEdit.CreatedByUser,
                UpdatedByUserId = productToEdit.UpdatedByUserId,
                UpdatedByUser = productToEdit.UpdatedByUser
            };

            return View(productViewModel);
        }

        // POST: ProductsController/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id, 
            [Bind("ProductID,ProductName,Quantity,SellingPricePerUnit,Image,CreatedByUserId,UpdatedByUserId,LastUpdatedOn")] ProductViewModel productViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("Create", "User not found.  Please log back in!");
            }

            if (!ModelState.IsValid)
            {
                return View(productViewModel);
            }

            // Load the data from the database for row that is to be edited.
            Product editProduct = await _context.Products.FindAsync(productViewModel.ProductID);
            if (editProduct == null)
            {
                return NotFound();
            }

            // update the properties of the Model - from the ViewModel
            editProduct.ProductName = productViewModel.ProductName;
            editProduct.SellingPricePerUnit = productViewModel.SellingPricePerUnit;
            editProduct.Quantity = productViewModel.Quantity;
            editProduct.LastUpdatedOn = DateTime.Now;
            editProduct.UpdatedByUserId = user.Id;

            /*
            // check if file has attached while submitting the Form
            if (Request.Form.Files.Count >= 1)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                // IFormFile file = productViewModel.ImageFile;

                // copy the file uploaded using the MemoryStream - into the Product.Image
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    editProduct.Image = dataStream.ToArray();
                }
            }
            */


            //for image posting to azure blob container
            IFormFile file = Request.Form.Files.FirstOrDefault();

            //var azureName = newProduct.ProductID.ToString() + ".jpg"; //this is to force it to be image if never set blobcontent type during uploadasync in helper method above, need to add .jpg in index.cshtml too, in the url path
            var azureName = editProduct.ProductID.ToString();
            // Check if file was uploaded, and is not an empty file.
            //if (file != null || file.Length > 0)
            if (file != null)
            {

                // Save the uploaded file on a temporary "UploadedImages" folder in wwwroot.
                var filepath = Path.Combine(_environment.WebRootPath, "UploadedImages", file.FileName);
                //var filepath = Path.Combine(_environment.WebRootPath, "UploadedImages", newProduct.ProductID.ToString());
                using (var stream = System.IO.File.Create(filepath))
                {
                    file.CopyToAsync(stream).Wait();
                }

                // Upload the image to the Blob Container
                //string imgBlobUri = await this.fSaveToBlobStorage(file.FileName, filepath);
                string imgBlobUri = await this.fSaveToBlobStorage(azureName, filepath);

                // Delete the uploaded image file from the temporary folder, as not needed any more.
                System.IO.File.Delete(filepath);

            }


            // update the database
            try
            {
                _context.Products.Update(editProduct);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception exp)
            {
                ModelState.AddModelError("Edit", "Unable to update the Database. Contact Admin!");
                _logger.LogError($"Edit Product failed: {exp.Message}");
                return View(productViewModel);
            }
        }

        // GET: ProductsController/Delete/5
        public async Task<ActionResult> DeleteAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the data from the database
            Product productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null)
            {
                return NotFound();
            }

            // Initialize the Viewmodel
            ProductViewModel productViewModel = new ProductViewModel()
            {
                ProductID = productToDelete.ProductID,
                ProductName = productToDelete.ProductName,
                SellingPricePerUnit = productToDelete.SellingPricePerUnit,
                Quantity = productToDelete.Quantity,
                Image = productToDelete.Image,

                LastUpdatedOn = productToDelete.LastUpdatedOn,
                CreatedByUserId = productToDelete.CreatedByUserId,
                CreatedByUser = productToDelete.CreatedByUser,
                UpdatedByUserId = productToDelete.UpdatedByUserId,
                UpdatedByUser = productToDelete.UpdatedByUser
            };

            return View(productViewModel);

        }

        // POST: ProductsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete
            (Guid id,
            [Bind("ProductId,ProductName,Quantity,SellingPricePerUnit,Image,CreatedByUserId,UpdatedByUserId,LastUpdatedOn")]
            ProductViewModel productViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("Delete", "User not found. Please log back in");
            }

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            foreach (var key in ModelState.Keys)
            {
                ModelState[key].Errors.Clear();
            }

            if (!ModelState.IsValid)
            {
                return View(productViewModel);
            }

            //load the data from db for row to be deleted
            Product deleteProduct = await _context.Products.FindAsync(id);
            if (deleteProduct == null)
            {
                return NotFound();
            }

            var storageConn = _config.GetValue<string>("ConnectionStrings:AzureConnection");

            // Get a reference to a Container
            BlobContainerClient blobContainerClient
                = new BlobContainerClient(storageConn, BlobContainerNAME);

            // Create the container if it does not exist - granting PUBLIC access.
            await blobContainerClient.CreateIfNotExistsAsync(
                Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            //delete the image file in azure blob container
            await blobContainerClient.DeleteBlobIfExistsAsync(id.ToString());


            // update the db
            try
            {
                _context.Products.Remove(deleteProduct);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception exp)
            {
                ModelState.AddModelError("Delete", "Unable to Delete from the db. Contact Admin");
                _logger.LogError($"Delete Product Failed: {exp.Message}");
                return View(productViewModel);
            }

        }

    }
}
