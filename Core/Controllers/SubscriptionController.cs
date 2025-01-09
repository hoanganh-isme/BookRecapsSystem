using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Subscription;
using BusinessObject.ViewModels.SubscriptionPackage;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Service;

namespace Core.Controllers
{
    [Route("api/subscription")]
    [ApiController]
    [AllowAnonymous]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public SubscriptionController(ISubscriptionService subscriptionService, IMapper mapper, ICurrentUserService currentUser)
        {
            _subscriptionService = subscriptionService;
            _mapper = mapper;
            _currentUser = currentUser;
        }
        [HttpPost("createsubscription")]
        public async Task<IActionResult> CreatePackage(Guid userId, Guid transactionId, Guid subscriptionPackageId)
        {
            var response = await _subscriptionService.CreateSubscription(userId, transactionId, subscriptionPackageId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("gethistorysubscription/{userId}")]
        public async Task<IActionResult> GetCurrentSubscription(Guid userId)
        {
            var response = await _subscriptionService.GetSubscriptionByUserId(userId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
