﻿using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class KnowledgebaseController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IKnowledgebaseService _knowledgebaseService;

        public KnowledgebaseController(ILocalizationService localizationService, IPermissionService permissionService, IKnowledgebaseService knowledgebaseService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._knowledgebaseService = knowledgebaseService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            return View();
        }

        public IActionResult NodeList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var categories = _knowledgebaseService.GetKnowledgebaseCategories();
            var articles = _knowledgebaseService.GetKnowledgebaseArticles();
            List<TreeNode> nodeList = new List<TreeNode>();

            List<ITreeNode> list = new List<ITreeNode>();
            list.AddRange(categories);
            list.AddRange(articles);

            foreach (var node in list)
            {
                if (string.IsNullOrEmpty(node.ParentCategoryId))
                {
                    var newNode = new TreeNode
                    {
                        id = node.Id,
                        text = node.Name,
                        isCategory = node.GetType() == typeof(KnowledgebaseCategory),
                        nodes = new List<TreeNode>()
                    };

                    FillChildNodes(newNode, list);

                    nodeList.Add(newNode);
                }
            }

            return Json(nodeList);
        }

        public void FillChildNodes(TreeNode parentNode, List<ITreeNode> nodes)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.id);
            foreach (var child in children)
            {
                var newNode = new TreeNode
                {
                    id = child.Id,
                    text = child.Name,
                    isCategory = child.GetType() == typeof(KnowledgebaseCategory),
                    nodes = new List<TreeNode>()
                };

                FillChildNodes(newNode, nodes);

                parentNode.nodes.Add(newNode);
            }
        }

        public IActionResult CreateCategory()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var model = new KnowledgebaseCategoryModel();
            model.Categories = _knowledgebaseService.GetKnowledgebaseCategories();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var knowledgebaseCategory = model.ToEntity();
                knowledgebaseCategory.CreatedOnUtc = DateTime.UtcNow;
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.InsertKnowledgebaseCategory(knowledgebaseCategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Added"));
                return continueEditing ? RedirectToAction("EditCategory", new { knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            var model = knowledgebaseCategory.ToModel();
            model.Categories = _knowledgebaseService.GetKnowledgebaseCategories();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(model.Id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseCategory = model.ToEntity(knowledgebaseCategory);
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.UpdateKnowledgebaseCategory(knowledgebaseCategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Updated"));
                return continueEditing ? RedirectToAction("EditCategory", new { id = knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            _knowledgebaseService.DeleteKnowledgebaseCategory(knowledgebaseCategory);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Deleted"));
            return RedirectToAction("List");
        }

        public IActionResult CreateArticle()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var model = new KnowledgebaseArticleModel();
            model.Categories = _knowledgebaseService.GetKnowledgebaseCategories();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var knowledgebaseArticle = model.ToEntity();
                knowledgebaseArticle.CreatedOnUtc = DateTime.UtcNow;
                knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.InsertKnowledgebaseArticle(knowledgebaseArticle);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Added"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditArticle(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            var model = knowledgebaseArticle.ToModel();
            model.Categories = _knowledgebaseService.GetKnowledgebaseCategories();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(model.Id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseArticle = model.ToEntity(knowledgebaseArticle);
                knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.UpdateKnowledgebaseArticle(knowledgebaseArticle);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Updated"));
                return continueEditing ? RedirectToAction("EditArticle", new { id = knowledgebaseArticle.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteArticle(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            _knowledgebaseService.DeleteKnowledgebaseArticle(knowledgebaseArticle);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Deleted"));
            return RedirectToAction("List");
        }
    }

    public class TreeNode
    {
        public string text { get; set; }

        public string id { get; set; }

        public List<TreeNode> nodes { get; set; }

        public bool isCategory { get; set; }
    }
}