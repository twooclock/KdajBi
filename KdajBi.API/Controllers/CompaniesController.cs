﻿using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize(Roles = "Super,Admin")]
    [ApiController]
    public class CompaniesController : _BaseController
    {
        public CompaniesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        [HttpPost("/api/companiestable")]
        public async Task<ActionResult<IQueryable<Company>>> CompaniesTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.Companies select a;

            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpGet("/api/companies")]
        public async Task<ActionResult<List<Company>>> GetCompanies()
        {
            
            List<Company> company;
            try
            {
                company = await _context.Companies.ToListAsync(); ;
            }
            catch (Exception ex)
            {
                throw;
            }

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        [HttpGet("/api/companies/{id}")]
        public async Task<ActionResult<Company>> GetCompany(long id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        [HttpPut("/api/companies/{id}")]
        public async Task<IActionResult> PutCompany(long id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("/api/company")]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            if (company.Id == 0)
            {
                company.CreatedUserID = _CurrentUserID();
                _context.Companies.Add(company);
                try
                {
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            else
            {

                var companyindb = _context.Companies.Single(c=> c.Id==company.Id);

                companyindb.UpdatedUserID = _CurrentUserID();
                companyindb.UpdatedDate = DateTime.Now;
                companyindb.Name = company.Name;
                companyindb.Active = (company.Active==null?true:company.Active);

                companyindb.Address = company.Address;
                companyindb.IsTaxpayer = company.IsTaxpayer;
                companyindb.Mobile = company.Mobile;
                companyindb.TaxIDNumber = company.TaxIDNumber;
                companyindb.TaxVATNumber = company.TaxVATNumber;
                companyindb.Zip = company.Zip;
                companyindb.ZipTown = company.ZipTown;

                _context.Entry(companyindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/companies/{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(long id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            foreach (var locItem in _context.Locations.Where(u => u.CompanyId == id).ToList())
            {
                var wplaces = _context.Workplaces.Where(u => u.LocationId == locItem.Id).ToList();
                foreach (var wpItem in wplaces)
                {
                    _context.WorkplaceScheduleExceptions.RemoveRange(_context.WorkplaceScheduleExceptions.Where(u => u.WorkplaceId == wpItem.Id).ToList());
                    var wpschedules = _context.WorkplaceSchedules.Where(u => u.WorkplaceId == wpItem.Id).ToList();
                    foreach (var schItem in wpschedules)
                    {
                        _context.Schedules.Remove(_context.Schedules.Find(schItem.ScheduleId));
                    }
                    _context.WorkplaceSchedules.RemoveRange(wpschedules);
                }
                _context.Workplaces.RemoveRange(wplaces);
            }
            _context.Settings.RemoveRange(_context.Settings.Where(u => u.CompanyId == id).ToList());
            _context.Clients.RemoveRange(_context.Clients.Where(u => u.CompanyId == id).ToList());
            _context.Services.RemoveRange(_context.Services.Where(u => u.CompanyId == id).ToList());
            _context.Companies.Remove(company);

            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool CompanyExists(long id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
