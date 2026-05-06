using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AdminPropertyRequestsController : Controller
    {
        private readonly IUnitOfWork _unitofWork;
        public AdminPropertyRequestsController( IUnitOfWork unitOfWork) 
        {
            _unitofWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var requestforadmin = _unitofWork.PropertyRequest.GetAll().Where(request => request.Requeststatus == "AgentAccepted").Include(user => user.User).ToList();

            if (requestforadmin == null) 
            {
                TempData["error"] = "Error while attempting to display the data";
            }

            return View(requestforadmin);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminApproveRequest(int ID)
        {
            var request = await _unitofWork.PropertyRequest.GetByIdAsync(ID);
            if (request == null)
            {
                return NotFound();
            }

            request.Requeststatus = "AdminAccepted";

            var newproperty = new Property
            {
                PropertyName = request.Property_Name,
                PropertyAddress = request.Property_Address,
                Price =  request.Property_Price,
                PropertyType = request.Property_Type,
                ExtendedDescription = request.Property_Description,
                PropertyGarages = request.Property_Garages,
                PropertyBedrooms = request.Property_Bedrooms,
                PropertyBathrooms = request.Property_Bathrooms,
                NearbySchools = request.Request_NearbySchools,
                NearbyShops = request.Request_NearbyShops,
                PropertyPets = request.Property_Pets,
            };

            await _unitofWork.Properties.AddAsync(newproperty);

            await _unitofWork.SaveChangesAsync();

            TempData["success"] = "New Property Request successfully added";

            return RedirectToAction("Index", "AdminProperties");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminRejectRequest(int ID)
        {
            var request = await _unitofWork.PropertyRequest.GetByIdAsync(ID);

            if (request == null)
            {
                return NotFound();
            }

            request.Requeststatus = "AdminRejected";

            _unitofWork.SaveChanges();

            TempData["success"] = "New Property Request rejected";

            return RedirectToAction("Index", "AdminProperties");
        }
    }
}
