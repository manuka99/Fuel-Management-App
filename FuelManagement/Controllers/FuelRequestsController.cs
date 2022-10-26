/*
    Manuka Yasas (IT19133850)
    Fuel request controller will expose apis that can be used to
    request fuel, token, get token info, next the next token in queue and much more functionaly.
    Please go through SwaggerOperation summary in each method to understand its usage
*/
using System;
using FuelManagement.Services;
using FuelManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FuelManagement.Util;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections;
using FuelManagement.Enums;

namespace FuelManagement.Controllers;

[Route("api/fuel-request")]
[ApiController]
public class FuelRequestsController : ControllerBase
{
    private readonly FuelRequestService _fuelRequestService;
    private readonly ShedService _shedService;
    private readonly TokenService _tokenService;

    public FuelRequestsController(FuelRequestService service, ShedService shedService, TokenService tokenService)
    {
        _fuelRequestService = service;
        _shedService = shedService;
        _tokenService = tokenService;
    }

    [SwaggerOperation(Summary = "Get the next available token id for a shed")]
    [HttpGet("next-token")]
    public async Task<ActionResult<IEnumerable<FuelRequests>>> GetNextTokenIdAll(string shedId)
    {
        var token = await _fuelRequestService.GetNextTokenId(shedId);
        return Ok(token + 1);
    }

    [SwaggerOperation(Summary = "Get queue information for a given shed and request token")]
    [HttpGet("queue-status")]
    public async Task<ActionResult<IEnumerable<FuelRequests>>> GetQueueInformationForToken(string shedId, string fuel, int tokenId)
    {
        var shed = await _shedService.GetByIdAsync(shedId);

        double availableQuota = 0;
        double holdingQuota = 0;
        int waitingTime = 0;

        if (shed == null) return BadRequest("Invalid shed");

        if (tokenId == 0 && fuel == null)
        {
            return BadRequest("Token Id or fuel type is required");
        }

        if (tokenId == 0)
        {
            var dispenseTime = 0;
            if (fuel == "petrol")
            {
                availableQuota = shed.petrolQTY;
                dispenseTime = shed.petrolDispenseTimePerVehicle;
            }
            else if (fuel != "diesel")
            {
                availableQuota = shed.dieselQTY;
                dispenseTime = shed.dieselDispenseTimePerVehicle;
            }
            else
            {
                return BadRequest("Invalid fuel");
            }

            var fuelRequests = await _fuelRequestService.GetByShedAndFuelType(shedId, fuel);
            foreach (var fuelRequest in fuelRequests)
            {
                holdingQuota += fuelRequest.fuelQTY;
            }
            waitingTime = (fuelRequests.Count + 1) * dispenseTime;
        }
        else
        {
            var fuelRequest = await _fuelRequestService.GetByTokenIdAndShed(tokenId, shedId);

            if (fuelRequest == null)
            {
                return BadRequest("Invalid token id");
            }
            else
            {
                var fuelRequests = new List<FuelRequests>();
                var dispenseTime = 0;
                if (fuelRequest.fuelType == "petrol")
                {
                    availableQuota = shed.petrolQTY;
                    dispenseTime = shed.petrolDispenseTimePerVehicle;
                    fuelRequests = await _fuelRequestService.GetByShedAndFuelType(shedId, fuelRequest.fuelType);
                }
                else if (fuelRequest.fuelType != "diesel")
                {
                    availableQuota = shed.petrolQTY;
                    dispenseTime = shed.dieselDispenseTimePerVehicle;
                    fuelRequests = await _fuelRequestService.GetByShedAndFuelType(shedId, fuelRequest.fuelType);
                }
                else
                {
                    return BadRequest("Invalid fuel for token");
                }

                var tokens = Array.Empty<int>();
                foreach (var fr in fuelRequests)
                {
                    tokens.Append(fr.tokenId);
                    holdingQuota += fr.fuelQTY;
                }
                Array.Sort(tokens);
                var searchedTokenIndex = Array.FindIndex(tokens, (x) => x == tokenId);
                if (searchedTokenIndex != -1)
                {
                    waitingTime = searchedTokenIndex * dispenseTime;
                }
                else return BadRequest("Invalid token id");
            }
        }

        return Ok(new { waitingTime, availableQuota, holdingQuota });
    }

    [SwaggerOperation(Summary = "Get fuel request by token id and shed id")]
    [HttpGet("token")]
    public async Task<ActionResult<FuelRequests>> GetByTokenIdAndShed(int tokenId, string shedId)
    {
        var fuelRequest = await _fuelRequestService.GetByTokenIdAndShed(tokenId, shedId);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [SwaggerOperation(Summary = "Get fuel request by request id")]
    [HttpGet("id")]
    public async Task<ActionResult<FuelRequests>> GetById(string id)
    {
        var fuelRequest = await _fuelRequestService.GetByIdAsync(id);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [SwaggerOperation(Summary = "Get all incompleted fuel requests for a given shed")]
    [HttpGet("shed")]
    [Authorize]
    public async Task<ActionResult<FuelRequests>> GetByShedId(string shedId)
    {
        var fuelRequest = await _fuelRequestService.GetByShedAsync(shedId);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [SwaggerOperation(Summary = "Get incompleted fuel request of a user for a given shed")]
    [HttpGet("shed-user")]
    [Authorize]
    public async Task<ActionResult<FuelRequests>> GetByRequestsOfUser(string shedId)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var fuelRequest = await _fuelRequestService.GetByShedUserAsync(shedId, authUser.Id);
        if (fuelRequest == null)
        {
            return NotFound();
        }
        return Ok(fuelRequest);
    }

    [SwaggerOperation(Summary = "Create new fuel request for a given shed")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(FuelRequests fuelRequest)
    {
        if (fuelRequest != null && fuelRequest.shed != null && fuelRequest.fuelType != null && fuelRequest.fuelQTY != null)
        {
            // validate user is authorized to make changes
            var authUser = _tokenService.getAuthUser(HttpContext);
            fuelRequest.user = authUser.Id;
            bool isRequestExist = await _fuelRequestService.checkUserRequestedShed(fuelRequest.shed, fuelRequest.user);
            if(isRequestExist) return BadRequest("There is request made already, complete it before creating another request");

            // validate shed
            var shedInfo = await _shedService.GetByIdAsync(fuelRequest.shed);
            if (shedInfo == null)
            {
                return NotFound();
            }

            //validate fuel quantity is correct and within limits
            switch (fuelRequest.fuelType)
            {
                case nameof(FuelType.PETROL):
                    if (fuelRequest.fuelQTY > shedInfo.maxPetrolRequestQTY)
                        return BadRequest("Exceeds maximum request quantity");
                    break;
                case nameof(FuelType.DIESEL):
                    if (fuelRequest.fuelQTY > shedInfo.maxDieselRequestQTY)
                        return BadRequest("Exceeds maximum request quantity");
                    break;
                default:
                    return BadRequest("Invalid fuel type");
                    //break;
            }

            fuelRequest.isApproved = true;
            fuelRequest.isCompleted = false;
            var result = await _fuelRequestService.CreateAsync(fuelRequest);
            if (result != null) Ok(result);
            else return BadRequest("An Error occured please try again");
        }
        return BadRequest("Required parameters were missing");
    }

    [SwaggerOperation(Summary = "Mark fuel request as completed")]
    [HttpPost("complete")]
    [Authorize]
    public async Task<IActionResult> CompleteFuelPump(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);
        var fuelRequest = await _fuelRequestService.GetByIdAsync(id);
        if (fuelRequest == null)
        {
            return NotFound();
        }

        var shedInfo = await _shedService.GetByIdAsync(fuelRequest.shed);
        if (shedInfo == null)
        {
            return NotFound();
        }

        if(shedInfo.owner != authUser.Id && fuelRequest.user != authUser.Id)
            return BadRequest(ErrorStatus.NOT_AUTHORIZED);

        if(fuelRequest.isCompleted) return Ok("Request was already updated as completed");

        fuelRequest.isCompleted = true;
        var result = await _fuelRequestService.UpdateAsync(id, fuelRequest);
        if (result) return Ok(result);
        else return BadRequest("An Error occured, pleaseÏ try again");
    }

    //[SwaggerOperation(Summary = "Update existing fuel request for a given request id")]
    //[HttpPut]
    //[Authorize]
    //public async Task<IActionResult> Update(string id, FuelRequests fuelRequest)
    //{
    //    // validate user is authorized to make changes
    //    var authUser = _tokenService.getAuthUser(HttpContext);

    //    var queriedRequest = await _fuelRequestService.GetByIdAsync(id);
    //    if (queriedRequest == null)
    //    {
    //        return NotFound();
    //    }

    //    if (authUser.Id != queriedRequest.user) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

    //    fuelRequest.Id = id;

    //    await _fuelRequestService.UpdateAsync(id, fuelRequest);
    //    return NoContent();
    //}

    [SwaggerOperation(Summary = "Delete existing fuel request for a given request id")]
    [HttpDelete("id")]
    [Authorize]
    public async Task<IActionResult> DeleteById(string id)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var fuelRequest = await _fuelRequestService.GetByIdAsync(id);
        if (fuelRequest == null)
        {
            return NotFound();
        }

        if (authUser.Id != fuelRequest.user) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        await _fuelRequestService.DeleteByIdAsync(id);
        return NoContent();
    }

    [SwaggerOperation(Summary = "Delete all incomplete fuel requests for a given shed")]
    [HttpDelete("shed")]
    [Authorize]
    public async Task<IActionResult> DeleteAllIncompleteRequestsPerShed(string shedId)
    {
        // validate user is authorized to make changes
        var authUser = _tokenService.getAuthUser(HttpContext);

        var shed = await _shedService.GetByIdAsync(shedId);
        if (shed == null)
        {
            return NotFound();
        }

        if (authUser.Id != shed.owner) return BadRequest(new { message = ErrorStatus.NOT_AUTHORIZED });

        await _fuelRequestService.DeleteAllIncompleteRequestsPerShedAsync(shedId);
        return Ok();
    }
}