using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using QIQI.EProjectFile;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using OpenEpl.TextECode.Internal.ProgramElems;

namespace OpenEpl.TextECode.Internal
{
    internal class FormInfoRestorer
    {
        private readonly TextECodeRestorer P;
        private readonly XmlDocument doc = new();
        private FormInfo formInfo;

        public FormInfoRestorer(TextECodeRestorer p)
        {
            P = p ?? throw new ArgumentNullException(nameof(p));
        }
        public void Load(Stream stream)
        {
            doc.Load(stream);
        }
        public UserFormDataType Restore()
        {
            var formXmlElem = doc.DocumentElement;
            formInfo = new FormInfo(P.AllocId(EplSystemId.Type_Form))
            {
                Name = formXmlElem.GetAttribute("名称"),
                Comment = formXmlElem.GetAttribute("备注"),
                Elements = new()
            };
            var formSelfControl = new FormControlInfo(P.AllocId(EplSystemId.Type_FormSelf))
            {
                DataType = 65537,
                Events = Array.Empty<KeyValuePair<int, int>>()
            };
            ReadProperty(formXmlElem, formSelfControl);
            formInfo.Elements.Add(formSelfControl);

            var formMenus = formXmlElem.ChildNodes
                .OfType<XmlElement>()
                .Where(x => x.Name == "窗口.菜单")
                .SingleOrDefault();
            if (formMenus != null)
            {
                HandleMenuChildren(formMenus, 0);
            }

            HandleControlChildren(formXmlElem, 0);

            var result = new UserFormDataType(P, formInfo);
            P.Forms.Add(result);
            return result;
        }

        public void HandleMenuChildren(XmlElement parentElem, int level)
        {
            foreach (var child in parentElem.ChildNodes.OfType<XmlElement>().Where(x => x.Name == "菜单"))
            {
                HandleMenu(child, level);
            }
        }

        public void HandleMenu(XmlElement xmlElem, int level)
        {
            var menu = new FormMenuInfo(P.AllocId(EplSystemId.Type_FormMenu))
            {
                DataType = 65539,
                Name = xmlElem.GetAttribute("名称"),
                Level = level
            };
            formInfo.Elements.Add(menu);
            ReadProperty(xmlElem, menu);
            HandleMenuChildren(xmlElem, level + 1);
        }

        public int HandleControl(XmlElement xmlElem, int parentId)
        {
            var dataTypeElem = (BaseDataTypeElem)P.TopLevelScope[ProgramElemName.Type(xmlElem.Name)];
            var control = new FormControlInfo(P.AllocId(EplSystemId.Type_FormControl))
            {
                DataType = dataTypeElem.Id,
                Name = xmlElem.GetAttribute("名称"),
                Comment = xmlElem.GetAttribute("备注"),
                Parent = parentId,
                Events = Array.Empty<KeyValuePair<int, int>>()
            };
            formInfo.Elements.Add(control);
            var children = new List<int>();
            HandleControlChildren(xmlElem, control.Id, children);
            var tabs = xmlElem.GetElementsByTagName($"{xmlElem.Name}.子夹").OfType<XmlElement>();
            var tabsEnumerator = tabs.GetEnumerator();
            if (tabsEnumerator.MoveNext())
            {
                var tab = tabsEnumerator.Current;
                HandleControlChildren(tab, control.Id, children);
                while (tabsEnumerator.MoveNext())
                {
                    children.Add(0);
                    tab = tabsEnumerator.Current;
                    HandleControlChildren(tab, control.Id, children);
                }
            }
            ReadProperty(xmlElem, control);
            control.Children = children.ToArray();
            return control.Id;
        }


        public void HandleControlChildren(XmlElement parentElem, int parentId)
        {
            foreach (var child in parentElem.ChildNodes.OfType<XmlElement>())
            {
                if (child.Name.StartsWith($"{parentElem.Name}."))
                {
                    continue;
                }
                HandleControl(child, parentId);
            }
        }

        public void HandleControlChildren(XmlElement parentElem, int parentId, List<int> childrenId)
        {
            foreach (var child in parentElem.ChildNodes.OfType<XmlElement>())
            {
                if (child.Name.StartsWith($"{parentElem.Name}."))
                {
                    continue;
                }
                childrenId.Add(HandleControl(child, parentId));
            }
        }

        private void ReadProperty(XmlElement xmlElement, FormMenuInfo menu)
        {
            menu.Text = xmlElement.GetAttribute("标题");
            menu.Selected = GetBoolAttribute(xmlElement, "选中", false);
            menu.Disable = GetBoolAttribute(xmlElement, "禁止", false);
            menu.Visible = GetBoolAttribute(xmlElement, "可视", true);
            menu.HotKey = GetIntAttribute(xmlElement, "快捷键", 0);
        }

        private void ReadProperty(XmlElement xmlElement, FormControlInfo control)
        {
            control.Left = GetIntAttribute(xmlElement, "左边", 0);
            control.Top = GetIntAttribute(xmlElement, "顶边", 0);
            control.Width = GetIntAttribute(xmlElement, "宽度", 0);
            control.Height = GetIntAttribute(xmlElement, "高度", 0);
            control.Tag = xmlElement.GetAttribute("标记");
            control.Disable = GetBoolAttribute(xmlElement, "禁止", false);
            control.Visible = GetBoolAttribute(xmlElement, "可视", true);
            control.Cursor = Convert.FromBase64String(xmlElement.GetAttribute("鼠标指针"));
            control.TabStop = GetBoolAttribute(xmlElement, "可停留焦点", true);
            control.TabIndex = GetIntAttribute(xmlElement, "停留顺序", 0);
            control.ExtensionData = Convert.FromBase64String(xmlElement.GetAttribute("扩展属性数据"));
        }

        private static bool GetBoolAttribute(XmlElement xmlElement, string name, bool defaultValue)
        {
            var attr = xmlElement.Attributes.GetNamedItem(name);
            if (attr is null)
            {
                return defaultValue;
            }
            return EStrToBool(attr.Value);
        }

        private static int GetIntAttribute(XmlElement xmlElement, string name, int defaultValue)
        {
            var attr = xmlElement.Attributes.GetNamedItem(name);
            if (attr is null)
            {
                return defaultValue;
            }
            return int.Parse(attr.Value);
        }

        private static bool EStrToBool(string value) => value.Trim().ToLowerInvariant() switch
        {
            "真" => true,
            "1" => true,
            "true" => true,
            "yes" => true,
            "on" => true,
            _ => false,
        };
    }
}