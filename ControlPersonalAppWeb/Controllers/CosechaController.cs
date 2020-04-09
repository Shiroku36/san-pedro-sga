using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlPersonalAppWeb.Controllers
{
    public class CosechaController : Controller
    {
        // GET: Cosecha
        public ActionResult Index()
        {
            return View();
        }

        // GET: Cosecha/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Cosecha/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Cosecha/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Cosecha/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Cosecha/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Cosecha/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Cosecha/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
