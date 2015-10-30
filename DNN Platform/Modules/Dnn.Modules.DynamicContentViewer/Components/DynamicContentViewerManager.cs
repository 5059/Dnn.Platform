﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    public class DynamicContentViewerManager : ServiceLocator<IDynamicContentViewerManager, DynamicContentViewerManager>, IDynamicContentViewerManager
    {
        #region Members

        private readonly IDynamicContentTypeManager _dynamicContentTypeManager;
        private readonly IDynamicContentItemManager _dynamicContentItemManager;
        private readonly IContentTemplateManager _contentTemplateManager;
        #endregion

        public DynamicContentViewerManager()
        {
            _dynamicContentTypeManager = DynamicContentTypeManager.Instance;
            _dynamicContentItemManager = DynamicContentItemManager.Instance;
            _contentTemplateManager = ContentTemplateManager.Instance;
        }

        public  DynamicContentItem GetOrCreateContentItem(ModuleInfo moduleInfo, int contentTypeId)
        {
            var contentItem = GetContentItem(moduleInfo, contentTypeId);
            if (contentItem != null)
            {
                return contentItem;
            }

            var contentType = _dynamicContentTypeManager.GetContentType(contentTypeId, moduleInfo.PortalID, true);
            contentItem = _dynamicContentItemManager.CreateContentItem(moduleInfo.PortalID, moduleInfo.TabID, moduleInfo.ModuleID, contentType);
            _dynamicContentItemManager.AddContentItem(contentItem);
            return contentItem;
        }

        public DynamicContentItem CreateDefaultContentItem(ModuleInfo moduleInfo)
        {
            var contentType = _dynamicContentTypeManager.GetContentTypes(moduleInfo.PortalID, true).SingleOrDefault(ct => ct.Name == "HTML");
            return _dynamicContentItemManager.CreateContentItem(moduleInfo.PortalID, moduleInfo.TabID, moduleInfo.ModuleID, contentType);
        }

        public DynamicContentItem GetContentItem(ModuleInfo moduleInfo)
        {
            var contentTypeId = GetContentTypeId(moduleInfo);
            return GetContentItem(moduleInfo, contentTypeId);
        }

        private DynamicContentItem GetContentItem(ModuleInfo moduleInfo, int contentTypeId)
        {
            return _dynamicContentItemManager.GetContentItems(moduleInfo.ModuleID, contentTypeId).SingleOrDefault();
        }

        public void UpdateContentItem(DynamicContentItem contentItem)
        {
            _dynamicContentItemManager.UpdateContentItem(contentItem);
        }

        public int GetContentTypeId(ModuleInfo moduleInfo)
        {
            var contentTypeId = moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
            if (contentTypeId == -1)
            {
                var contentType = _dynamicContentTypeManager.GetContentTypes(moduleInfo.PortalID, true).Single(ct => ct.Name == "HTML");
                contentTypeId = contentType.ContentTypeId;
            }

            return contentTypeId;
        }

        public int GetViewTemplateId(ModuleInfo moduleInfo)
        {
            var templateId = moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ViewTemplateId, -1);
            if (templateId == -1)
            {
                var contentTypeId = moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
                if (contentTypeId == -1)
                {
                    var contentType = _dynamicContentTypeManager.GetContentTypes(moduleInfo.PortalID, true).Single(ct => ct.Name == "HTML");
                    var template = _contentTemplateManager.GetContentTemplates(contentType.ContentTypeId, moduleInfo.PortalID, true).Single(t => t.Name == "Getting Started");
                    templateId = template.TemplateId;                    
                }
            }

            return templateId;
        }

        public int GetEditTemplateId(ModuleInfo moduleInfo)
        {
            return moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_EditTemplateId, -1);
        }

        protected override System.Func<IDynamicContentViewerManager> GetFactory()
        {
            return () => new DynamicContentViewerManager();
        }
    }
}