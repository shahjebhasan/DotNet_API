using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.DTO;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private APIResponse _response;
        public MenuItemController(ApplicationDBContext db)
        {
            _db = db;
            _response = new APIResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _db.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(Response);
        }

        [HttpGet("{id:int}", Name ="GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if(id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(Response);
            }
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(x=>x.Id == id);
            if(menuItem == null)
            {
                _response.StatusCode  = HttpStatusCode.BadRequest;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(Response);
        }
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateMeuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price = menuItemCreateDTO.Price,
                        Category = menuItemCreateDTO.Category,
                        SpecialTag = menuItemCreateDTO.SpecialTag,
                        Description = menuItemCreateDTO.Description,
                        Image = menuItemCreateDTO.Image,
                    };
                    _db.MenuItems.Add(menuItemToCreate);
                    _db.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                    if (menuItemFromDb == null)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    menuItemFromDb.Name = menuItemUpdateDTO.Name;
                    menuItemFromDb.Price = menuItemUpdateDTO.Price;
                    menuItemFromDb.Category = menuItemUpdateDTO.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDTO.SpecialTag;
                    menuItemFromDb.Description = menuItemUpdateDTO.Description;
                    menuItemFromDb.Description = menuItemUpdateDTO.Description;

                    _db.MenuItems.Update(menuItemFromDb);
                    _db.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);

                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }

            return _response;
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<APIResponse>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                _db.MenuItems.Remove(menuItemFromDb);
                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }

            return _response;
        }
}
}
