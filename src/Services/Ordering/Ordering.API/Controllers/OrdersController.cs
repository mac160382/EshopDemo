﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Services.Ordering.API.Application.Commands;
using Demo.Services.Ordering.API.Application.Queries;
using Demo.Services.Ordering.API.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Services.Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IOrderQueries _orderQueries;
        private readonly IIdentityService _identityService;

        public OrdersController(IMediator mediator, IOrderQueries orderQueries, IIdentityService identityService)
        {

            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        [Route("new")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody]CreateOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;
            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestCreateOrder);
            }
            else
            {
                // If no x-requestid header is found we process the order anyway. This is just temporary to not break existing clients
                // that aren't still updated. When all clients were updated this could be removed.
                result = await _mediator.SendAsync(command);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("{orderId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var order = await _orderQueries.GetOrder(orderId);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderQueries.GetOrders();

            return Ok(orders);
        }

        [Route("cardtypes")]
        [HttpGet]
        public async Task<IActionResult> GetCardTypes()
        {
            var cardTypes = await _orderQueries.GetCardTypes();

            return Ok(cardTypes);
        }        
    }
}


