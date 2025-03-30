using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.DTO;
using System.Net;

namespace RedMango_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MenuItemController : ControllerBase
	{
		private readonly ApplicationDBContext _db;
		public ApiResponse _response;
        public MenuItemController(ApplicationDBContext db)
        {
            _db = db;
			_response = new ApiResponse();
        }

		[HttpGet]
		public async Task<IActionResult> GetMenuItems()
		{
			_response.Result = _db.MenuItems.ToList();
			_response.StatusCode = HttpStatusCode.OK;
			
			return Ok(_response);
		}

		[HttpGet("{Id:int}", Name = "GetMenuItem")]
		public async Task<IActionResult> GetMenuItem(int Id)
		{
			if (Id <= 0)
			{
				_response.IsSuccess = false;
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.ErrorList = new List<string>() { "Id should not be equal to or lass than Zero" };
				return BadRequest(_response);
			}

			MenuItem menuItem = _db.MenuItems.FirstOrDefault(mi => mi.Id == Id);
			if (menuItem == null) 
			{ 
				_response.IsSuccess = false;
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.ErrorList = new List<string>() { "Id is not valid" };
				return BadRequest(_response);
			}

			_response.Result = menuItem;
			_response.StatusCode = HttpStatusCode.OK;

			return Ok(_response);
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItem)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (menuItem.File == null || menuItem.File.Length == 0)
					{
						_response.IsSuccess = false;
						_response.StatusCode = HttpStatusCode.BadRequest;
						return BadRequest(_response);
					}

					//for Image to store in Azure Blob storage
					//string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";

					MenuItem menuItemToCreate = new MenuItem()
					{
						Name = menuItem.Name,
						Description = menuItem.Description,
						SpecialTag = menuItem.SpecialTag,
						Category = menuItem.Category,
						Price = menuItem.Price,
						Image = menuItem.File.FileName.ToString()
						//,Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemCreateDTO.File)
					};

					_db.MenuItems.Add(menuItemToCreate);
					_db.SaveChanges();
					_response.Result = menuItemToCreate;
					_response.StatusCode = HttpStatusCode.Created;
					return CreatedAtAction(nameof(GetMenuItem), new { id = menuItemToCreate.Id }, _response);
				}
				else
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
				}
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.ErrorList = new List<string> { ex.Message };
			}

			return Ok(_response);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItem)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (id != menuItem.Id || menuItem == null)
					{
						_response.IsSuccess = false;
						_response.StatusCode = HttpStatusCode.BadRequest;
						_response.ErrorList = new List<string> { "Id is not Matching the request ID - Bad Request" };
						return BadRequest(_response);
					}

					MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
					if (menuItemFromDb == null)
					{
						_response.IsSuccess = false;
						_response.StatusCode = HttpStatusCode.NotFound;
						_response.ErrorList = new List<string> { "Id should be valid - Id Not Found" };
						return NotFound(_response);
					}

					//if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
					//{
					//	string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";
					//	await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Container);
					//	menuItemFromDb.Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemUpdateDTO.File);
					//}

					menuItemFromDb.Name = menuItem.Name;
					menuItemFromDb.Description = menuItem.Description;
					menuItemFromDb.Category = menuItem.Category;
					menuItemFromDb.Price = menuItem.Price;
					menuItemFromDb.SpecialTag = menuItem.SpecialTag;

					_db.MenuItems.Update(menuItemFromDb);
					_db.SaveChanges();
					_response.Result = menuItemFromDb;
					_response.StatusCode = HttpStatusCode.NoContent;
					return Ok(_response);
				}
				else
				{
					_response.IsSuccess = false;
					_response.ErrorList = new List<string> { "Update Model should be valid - Model not valid" };
				}
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorList = new List<string> { ex.Message };
			}

			return Ok(_response);
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
		{
			try
			{
				if (id <= 0)
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.ErrorList = new List<string> { "Id should be valid - Id Not Valid" };
					return BadRequest(_response);
				}

				MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
				if (menuItemFromDb == null)
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.ErrorList = new List<string> { "Id should be valid - Id Not Found" };
					return NotFound(_response);
				}

				//await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Container);
				//int milliseconds = 2000;
				//Thread.Sleep(milliseconds);

				_db.MenuItems.Remove(menuItemFromDb);
				_db.SaveChanges();
				_response.StatusCode = HttpStatusCode.NoContent;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorList = new List<string> { ex.Message };
			}

			return Ok(_response);
		}
	}
}
