using Microsoft.Extensions.Logging;
using OpenEpl.ELibInfo;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace OpenEpl.TextECode.Internal
{
    internal class FormInfoGenerator
    {
        private readonly XmlDocument doc = new();

        public FormInfoGenerator(FormInfo formInfo, LibraryRefInfo[] libraryRefInfo, IdToNameMap idToNameMap, ILoggerFactory factory)
        {
            var logger = factory.CreateLogger<FormInfoGenerator>();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            var formXmlElem = doc.CreateElement("窗口");
            formXmlElem.SetAttribute("名称", idToNameMap.GetUserDefinedName(formInfo.Id));
            if (!string.IsNullOrEmpty(formInfo.Comment))
            {
                formXmlElem.SetAttribute("备注", formInfo.Comment);
            }
            var formSelfControl = (FormControlInfo)formInfo.Elements.Single(x => EplSystemId.GetType(x.Id) == EplSystemId.Type_FormSelf);
            WriteProperty(formXmlElem, formSelfControl);
            doc.AppendChild(formXmlElem);

            var menus = formInfo.Elements.Where(x => x is FormMenuInfo).Cast<FormMenuInfo>().ToList();
            if (menus.Count != 0)
            {
                var menuRoot = doc.CreateElement("窗口.菜单");
                var menuStack = new Stack<XmlElement>();
                menuStack.Push(menuRoot);
                foreach (var menu in menus)
                {
                    while (menu.Level < menuStack.Count - 1)
                    {
                        menuStack.Pop();
                    }
                    var menuXmlElem = doc.CreateElement("菜单");
                    menuXmlElem.SetAttribute("名称", idToNameMap.GetUserDefinedName(menu.Id));
                    menuStack.Peek().AppendChild(menuXmlElem);
                    WriteProperty(menuXmlElem, menu);
                    menuStack.Push(menuXmlElem);
                }
                formXmlElem.AppendChild(menuRoot);
            }

            var controls = formInfo.Elements.OfType<FormControlInfo>().Where(x => x != formSelfControl).ToList();
            var controlIdMap = controls.ToDictionary(x => x.Id);
            var taskPairs = new Stack<(int[] items, XmlElement parentXmlElem)>();
            taskPairs.Push((controls.Where(x => x.Parent == 0).Select(x => x.Id).ToArray(), formXmlElem));
            while (taskPairs.Count != 0)
            {
                var (items, parentXmlElem) = taskPairs.Pop();
                foreach (var item in items.Select(x => controlIdMap[x]))
                {
                    EplSystemId.DecomposeLibDataTypeId(item.DataType, out var lib, out var type);
                    var dataTypeInfo = idToNameMap.LibDefinedName.ElementAtOrDefault(lib)
                        ?.DataTypes.ElementAtOrDefault(type);
                    var itemXmlElemName = dataTypeInfo?.Name;
                    if (itemXmlElemName is null)
                    {
                        logger.LogError("未知的窗口数据类型: {0}.{1}", libraryRefInfo[lib].Name, type);
                        itemXmlElemName = $"未知类型.{libraryRefInfo[lib].Name}.{type}";
                    }
                    var itemXmlElem = doc.CreateElement(itemXmlElemName);
                    itemXmlElem.SetAttribute("名称", idToNameMap.GetUserDefinedName(item.Id));
                    if (!string.IsNullOrEmpty(formInfo.Comment))
                    {
                        itemXmlElem.SetAttribute("备注", item.Comment);
                    }
                    parentXmlElem.AppendChild(itemXmlElem);
                    bool isTabControl;
                    if (dataTypeInfo is null)
                    {
                        // fallback
                        isTabControl = item.Children.Contains(0);
                        if (isTabControl)
                        {
                            logger.LogWarning("未知的数据类型: {0}.{1}，判定为Tab容器类型", libraryRefInfo[lib].Name, type);
                        }
                        else if (item.Children.Length != 0)
                        {
                            logger.LogWarning("未知的数据类型: {0}.{1}，判定为普通容器类型", libraryRefInfo[lib].Name, type);
                        }
                    }
                    else
                    {
                        isTabControl = dataTypeInfo is ELibComponentInfo componentInfo && componentInfo.IsTabControl;
                    }
                    if (isTabControl)
                    {
                        var currentTabXmlElem = doc.CreateElement($"{itemXmlElem.Name}.子夹");
                        var currentTabChildren = new List<int>();
                        itemXmlElem.AppendChild(currentTabXmlElem);
                        foreach (var childId in item.Children)
                        {
                            if (childId == 0)
                            {
                                taskPairs.Push((currentTabChildren.ToArray(), currentTabXmlElem));

                                currentTabXmlElem = doc.CreateElement($"{itemXmlElem.Name}.子夹");
                                itemXmlElem.AppendChild(currentTabXmlElem);
                                currentTabChildren.Clear();
                            }
                            else
                            {
                                currentTabChildren.Add(childId);
                            }
                        }
                        taskPairs.Push((currentTabChildren.ToArray(), currentTabXmlElem));
                    }
                    else
                    {
                        taskPairs.Push((item.Children, itemXmlElem));
                    }
                    WriteProperty(itemXmlElem, item);
                }
            }
        }

        private void WriteProperty(XmlElement xmlElement, FormMenuInfo menu)
        {
            xmlElement.SetAttribute("标题", menu.Text);
            if (menu.Selected)
            {
                xmlElement.SetAttribute("选中", BoolToEStr(menu.Selected));
            }
            if (menu.Disable)
            {
                xmlElement.SetAttribute("禁止", BoolToEStr(menu.Disable));
            }
            if (!menu.Visible)
            {
                xmlElement.SetAttribute("可视", BoolToEStr(menu.Visible));
            }
            if (menu.HotKey != 0)
            {
                xmlElement.SetAttribute("快捷键", menu.HotKey.ToString());
            }
        }

        private void WriteProperty(XmlElement xmlElement, FormControlInfo control)
        {
            xmlElement.SetAttribute("左边", control.Left.ToString());
            xmlElement.SetAttribute("顶边", control.Top.ToString());
            xmlElement.SetAttribute("宽度", control.Width.ToString());
            xmlElement.SetAttribute("高度", control.Height.ToString());
            if (!string.IsNullOrEmpty(control.Tag))
            {
                xmlElement.SetAttribute("标记", control.Tag);
            }
            if (control.Disable)
            {
                xmlElement.SetAttribute("禁止", BoolToEStr(control.Disable));
            }
            if (!control.Visible)
            {
                xmlElement.SetAttribute("可视", BoolToEStr(control.Visible));
            }
            xmlElement.SetAttribute("鼠标指针", Convert.ToBase64String(control.Cursor));
            xmlElement.SetAttribute("可停留焦点", BoolToEStr(control.TabStop));
            if (control.TabIndex != 0)
            {
                xmlElement.SetAttribute("停留顺序", control.TabIndex.ToString());
            }
            xmlElement.SetAttribute("扩展属性数据", Convert.ToBase64String(control.ExtensionData));
        }

        public void Save(Stream writer)
        {
            doc.Save(writer);
        }

        private static string BoolToEStr(bool value) => value ? "真" : "假";
    }
}
