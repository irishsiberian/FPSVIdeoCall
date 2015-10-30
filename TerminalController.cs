using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FPSVideoCallApplication.Common;
using FPSVideoCallApplication.Models;
using FPSVideoCallApplication.Models.Terminal;
using MvcContrib.UI.Grid;

namespace FPSVideoCallApplication.Controllers
{
    
    [HandleError]
    public partial class TerminalController : BaseController
    {
        readonly TerminalRepository _selectionRepository = new TerminalRepository();

        /// <summary>
        /// Возвращает страницу с сортированными данными
        /// </summary>
        /// <param name="page">номер страницы для отображения</param>
        /// <param name="sortOptions">данные о сортировке</param>
        /// <param name="number">Номер видеотерминала</param>
        /// <param name="regionId">Идентификатор региона</param>
        /// <param name="correctionFacilityId">Идентификатор учреждения</param>
        /// <param name="terminalAddress">Адрес установки терминала</param>
        /// <returns>Список отсортированных видеотерминалов</returns>
        [Authorize(Roles = "Administrators, Managers, CallCenterUsers")]
        public virtual ActionResult List(int? page, GridSortOptions sortOptions, string number, int? regionId, int? correctionFacilityId, string terminalAddress)
        {
            // реализация поиска и фильтрации
            //http://www.codeproject.com/KB/aspnet/Grid_Paging_In_MVC3.aspx

            var dbFacilities = _selectionRepository.GetAllObjects().Where(terminal => terminal.IsDeleted == false);
            if (!UserHelper.Instance.IsAdministrator && !UserHelper.Instance.IsManager && !UserHelper.Instance.IsCallCenterUser) //простым смертным показывать только активные терминалы
                dbFacilities = dbFacilities.Where(terminal => terminal.IsActive == true);
            var facilities = dbFacilities.Select(dbTerminal => new TerminalListModel
                                                                   {
                                                                       Id = dbTerminal.Id,
                                                                       PhoneNumber = dbTerminal.PhoneNumber,
                                                                       IsPublic = dbTerminal.IsPublic,
                                                                       CorrectionFacilityId = dbTerminal.CorrectionFacilityId,
                                                                       CorrectionFacilityName = dbTerminal.CorrectionFacility.Name,
                                                                       InstallationAddress = dbTerminal.CorrectionFacility.Address,
                                                                       RegionId = dbTerminal.RegionId,
                                                                       IsActive = dbTerminal.IsActive
                                                                   });

            var pagedViewModel = new PagedViewModel<TerminalListModel>
            {
                ViewData = ViewData,
                Query = facilities,
                GridSortOptions = sortOptions,
                DefaultSortColumn = "PhoneNumber",
                Page = page,
                PageSize = 10
            }
            .AddFilter("number", number, t => t.PhoneNumber.Contains(number))
            .AddFilter("RegionId", regionId, t => t.RegionId == regionId, RegionController.GetDBRegionList(null), "Id", "Name")
            .AddFilter("CorrectionFacilityId", correctionFacilityId, t => t.CorrectionFacilityId == correctionFacilityId, CorrectionFacilityController.GetDbCorrectionFacilityList(-1, null), "Id", "Name")
            .AddFilter("terminalAddress", terminalAddress, t => t.InstallationAddress.ToLower().Contains(terminalAddress.ToLower()))
            .Setup();

            return View(pagedViewModel);
        }


        /// <summary>
        /// Возвращает список видеотерминалов
        /// </summary>
        /// <param name="facilityId">Идентификатор учреждения</param>
        /// <param name="onlyWithPublicTerminals">true - только учреждения с видеотерминалами ПКД, false - только учреждения с видеотерминалами для осужденных, null - все видеотерминалы учреждения</param>
        /// <returns></returns>
        [OutputCache(Duration = 60)]
        public static IEnumerable<SelectListItem> GetTerminalsByCorrectionFacility(int facilityId, bool? onlyWithPublicTerminals)
        {
            using (TerminalRepository repository = new TerminalRepository())
            {
                var terminals =
                    repository.GetAllObjects()
                        .Where(t => t.CorrectionFacilityId == facilityId && t.IsDeleted == false && t.IsActive == true);

                if (onlyWithPublicTerminals != null)
                {
                    bool onlyPublic = (bool)onlyWithPublicTerminals;
                    terminals = terminals.Where(t => t.IsPublic == onlyPublic);
                }

                var returnList = terminals.OrderBy(t => t.PhoneNumber)
                        .AsEnumerable()
                        .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.PhoneNumber })
                        .ToList();

                return returnList;
            }
        }

        

        [HttpGet]
        public virtual JsonResult GetJsonConvictTerminalsByCorrectionFacility(int facilityId)
        {
            var terminals = GetTerminalsByCorrectionFacility(facilityId, false);
            return Json(terminals, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public virtual JsonResult GetJsonPublicTerminalsByCorrectionFacility(int facilityId)
        {
            var terminals = GetTerminalsByCorrectionFacility(facilityId, true);
            return Json(terminals, JsonRequestBehavior.AllowGet);

        }


        //
        // GET: /Terminal/Details/5
        /// <summary>
        /// Отображает детали видеотерминала
        /// </summary>
        /// <param name="id">Идентификатор видеотерминала</param>
        /// <returns>Видеотерминал</returns>
        [Authorize]
        public virtual ActionResult Details(int id)
        {
            var dbTerminal = _selectionRepository.GetObjectById(id);

            if (dbTerminal.IsDeleted)
                this.WriteInformation("Данный видеотерминал не используется.");

            var terminal = new TerminalModel
                                         {
                                             Id = dbTerminal.Id,
                                             PhoneNumber = dbTerminal.PhoneNumber,
                                             IsPublic = dbTerminal.IsPublic,
                                             RegionId = dbTerminal.RegionId,
                                             RegionName = dbTerminal.Region.Name,
                                             CorrectionFacilityId = dbTerminal.CorrectionFacility.Id,
                                             CorrectionFacilityName = dbTerminal.CorrectionFacility.Name,
                                             IsActive = dbTerminal.IsActive
                                         };

            ViewBag.RouteDictionaryForList = Request.QueryString.ToRouteDictionary();

            return View(terminal);
        }

        [Authorize (Roles = "Administrators, Managers, CallCenterUsers")]
        public virtual ActionResult TimeVerification()
        {
            ViewBag.Regions = RegionController.GetRegionList(false);

            ViewBag.RegionsWithPublicTerminals = RegionController.GetRegionList(true);

            TimeVerificationModel timeVerificationModel = new TimeVerificationModel();

            return View(timeVerificationModel);
        }

        ////
        //// GET: /Terminal/Register
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators, Managers")]
        public virtual ActionResult Create()
        {
            ViewBag.Regions = RegionController.GetRegionList(null);

            TerminalModel terminal = new TerminalModel()
            {
                IsActive = true //по умолчанию терминал активен
            };

            return View(terminal);
        }

        ////
        //// POST: /Terminal/Register
        [HttpPost]
        [Authorize(Roles = "Administrators, Managers")]
        public virtual ActionResult Create(TerminalModel terminal)
        {
            try
            {
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    TerminalRepository insertionRepository = new TerminalRepository(unitOfWork.DataContext);
                    DBTerminal dbTerminal = new DBTerminal
                                                {
                                                    Id = terminal.Id,
                                                    PhoneNumber = terminal.PhoneNumber,
                                                    IsPublic = terminal.IsPublic,
                                                    RegionId = terminal.RegionId,
                                                    CorrectionFacilityId = terminal.CorrectionFacilityId,
                                                    Description = terminal.Description,
                                                    IsActive = terminal.IsActive
                                                };
                    insertionRepository.InsertObject(dbTerminal);
                    unitOfWork.Commit();

                    // обновляем идентификатор
                    terminal.Id = dbTerminal.Id;
                }

                // сохраняем идентификатор видеотерминала
                this.WriteInformation("Видеотерминал с идентификатором \"{0}\" добавлен.", terminal.Id);

                return RedirectToAction("Create");
            }
            catch
            {
                return View();
            }
        }


        ////
        //// GET: /CorrectionFacility/Edit/5
        [Authorize(Roles = "Administrators, Managers")]
        public virtual ActionResult Edit(int id)
        {
            DBTerminal dbTerminal = _selectionRepository.GetObjectById(id);


            if (dbTerminal.IsDeleted)
                return RedirectToAction("Details", new { id = id });

            TerminalModel terminal = new TerminalModel()
                                                 {
                                                     Id = dbTerminal.Id,
                                                     PhoneNumber = dbTerminal.PhoneNumber,
                                                     IsPublic = dbTerminal.IsPublic,
                                                     RegionId = dbTerminal.RegionId,
                                                     CorrectionFacilityId = dbTerminal.CorrectionFacilityId,
                                                     Description = dbTerminal.Description,
                                                     IsActive = dbTerminal.IsActive
                                                 };

            ViewBag.Regions = RegionController.GetRegionList(null);
            ViewBag.CorrectionFacilities = CorrectionFacilityController.GetCorrectionFacilitiesByRegion(dbTerminal.RegionId, null);

            return View(terminal);
        }

        ////
        //// POST: /CorrectionFacility/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrators, Managers")]
        public virtual ActionResult Edit(TerminalModel terminal)
        {
            try
            {
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    TerminalRepository updateRepository = new TerminalRepository(unitOfWork.DataContext);
                    DBTerminal dbTerminal = updateRepository.GetObjectById(terminal.Id);

                    if (dbTerminal == null)
                    {
                        this.WriteErrorMessage("Видеотерминал с идентификатором {0} не найден.", terminal.Id);
                    }
                    else
                    {
                        dbTerminal.PhoneNumber = terminal.PhoneNumber;
                        dbTerminal.IsPublic = terminal.IsPublic;
                        dbTerminal.RegionId = terminal.RegionId;
                        dbTerminal.CorrectionFacilityId = terminal.CorrectionFacilityId;
                        dbTerminal.Description = terminal.Description;
                        dbTerminal.IsActive = terminal.IsActive;

                        updateRepository.UpdateObject(dbTerminal);
                        unitOfWork.Commit();

                        // сохраняем номер телефона
                        this.WriteInformation("Видеотерминал с идентификатором \"{0}\" сохранен.", terminal.Id);
                    }

                }

                return RedirectToAction("Details", new { id = terminal.Id });
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /CorrectionFacility/Delete/5
        [Authorize(Roles = "Administrators, Managers")]
        public virtual ActionResult Delete(int id)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                TerminalRepository repository = new TerminalRepository(unitOfWork.DataContext);
                DBTerminal dbTerminal = repository.GetObjectById(id);

                if (dbTerminal == null)
                {
                    this.WriteErrorMessage("Видеотерминал с идентификатором {0} не найден.", id);
                }
                else
                {
                    repository.DeleteObject(dbTerminal);
                    // сохраняем идентификатор удаленного видеотерминала
                    this.WriteInformation("Видеотерминал с идентификатором \"{0}\" удален.", dbTerminal.Id);
                    unitOfWork.Commit();
                }

            }
            return RedirectToAction("List");
        }
    }
}
