// <copyright file="AXNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageAccessibility
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Helpers.Json;

    internal class AXNode
    {
        private readonly string name;
        private readonly bool richlyEditable;
        private readonly bool editable;
        private readonly bool hidden;
        private readonly string role;
        private readonly bool ignored;
        private bool? cachedHasFocusableChild;

        private AXNode(AccessibilityGetFullAXTreeResponse.AXTreeNode payload)
        {
            this.Payload = payload;

            this.name = payload.Name != null ? payload.Name.Value.ToObject<string>() : string.Empty;
            this.role = payload.Role != null ? payload.Role.Value.ToObject<string>() : "Unknown";
            this.ignored = payload.Ignored;

            this.richlyEditable = payload.Properties?.FirstOrDefault(p => p.Name == "editable")?.Value.Value.ToObject<string>() == "richtext";
            this.editable |= this.richlyEditable;
            this.hidden = payload.Properties?.FirstOrDefault(p => p.Name == "hidden")?.Value.Value.ToObject<bool>() == true;
            this.Focusable = payload.Properties?.FirstOrDefault(p => p.Name == "focusable")?.Value.Value.ToObject<bool>() == true;
        }

        public List<AXNode> Children { get; } = new();

        public bool Focusable { get; set; }

        internal AccessibilityGetFullAXTreeResponse.AXTreeNode Payload { get; }

        internal static AXNode CreateTree(IEnumerable<AccessibilityGetFullAXTreeResponse.AXTreeNode> payloads)
        {
            var nodeById = new Dictionary<string, AXNode>();
            foreach (var payload in payloads)
            {
                nodeById[payload.NodeId] = new AXNode(payload);
            }

            foreach (var node in nodeById.Values)
            {
                foreach (var childId in node.Payload.ChildIds)
                {
                    node.Children.Add(nodeById[childId]);
                }
            }

            return nodeById.Values.FirstOrDefault();
        }

        internal AXNode Find(Func<AXNode, bool> predicate)
        {
            if (predicate(this))
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.Find(predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        internal bool IsLeafNode()
        {
            if (this.Children.Count == 0)
            {
                return true;
            }

            // These types of objects may have children that we use as internal
            // implementation details, but we want to expose them as leaves to platform
            // accessibility APIs because screen readers might be confused if they find
            // any children.
            if (this.IsPlainTextField() || this.IsTextOnlyObject())
            {
                return true;
            }

            // Roles whose children are only presentational according to the ARIA and
            // HTML5 Specs should be hidden from screen readers.
            // (Note that whilst ARIA buttons can have only presentational children, HTML5
            // buttons are allowed to have content.)
            switch (this.role)
            {
                case "doc-cover":
                case "graphics-symbol":
                case "img":
                case "image":
                case "Meter":
                case "scrollbar":
                case "slider":
                case "separator":
                case "progressbar":
                    return true;
            }

            // Here and below: Android heuristics
            if (this.HasFocusableChild())
            {
                return false;
            }

            if (this.Focusable && !string.IsNullOrEmpty(this.name))
            {
                return true;
            }

            if (this.role == "heading" && !string.IsNullOrEmpty(this.name))
            {
                return true;
            }

            return false;
        }

        internal bool IsControl()
        {
            switch (this.role)
            {
                case "button":
                case "checkbox":
                case "ColorWell":
                case "combobox":
                case "DisclosureTriangle":
                case "listbox":
                case "menu":
                case "menubar":
                case "menuitem":
                case "menuitemcheckbox":
                case "menuitemradio":
                case "radio":
                case "scrollbar":
                case "searchbox":
                case "slider":
                case "spinbutton":
                case "switch":
                case "tab":
                case "textbox":
                case "tree":
                case "treeitem":
                    return true;
                default:
                    return false;
            }
        }

        internal bool IsInteresting(bool insideControl)
        {
            if (this.role == "Ignored" || this.hidden || this.ignored)
            {
                return false;
            }

            if (this.Focusable || this.richlyEditable)
            {
                return true;
            }

            // If it's not focusable but has a control role, then it's interesting.
            if (this.IsControl())
            {
                return true;
            }

            // A non focusable child of a control is not interesting
            if (insideControl)
            {
                return false;
            }

            return this.IsLeafNode() && !string.IsNullOrEmpty(this.name);
        }

        internal SerializedAXNode Serialize()
        {
            var properties = new Dictionary<string, JsonElement?>();

            if (this.Payload.Properties != null)
            {
                foreach (var property in this.Payload.Properties)
                {
                    properties[property.Name.ToLower(CultureInfo.CurrentCulture)] = property.Value.Value;
                }
            }

            if (this.Payload.Name != null)
            {
                properties["name"] = this.Payload.Name.Value;
            }

            if (this.Payload.Value != null)
            {
                properties["value"] = this.Payload.Value.Value;
            }

            if (this.Payload.Description != null)
            {
                properties["description"] = this.Payload.Description.Value;
            }

            var node = new SerializedAXNode
            {
                Role = this.role,
                Name = properties.GetValue("name")?.ToObject<string>(),
                Value = properties.GetValue("value")?.ToObject<object>()?.ToString(),
                Description = properties.GetValue("description")?.ToObject<string>(),
                KeyShortcuts = properties.GetValue("keyshortcuts")?.ToObject<string>(),
                RoleDescription = properties.GetValue("roledescription")?.ToObject<string>(),
                ValueText = properties.GetValue("valuetext")?.ToObject<string>(),
                Disabled = properties.GetValue("disabled")?.ToObject<bool>() ?? false,
                Expanded = properties.GetValue("expanded")?.ToObject<bool>() ?? false,

                // RootWebArea's treat focus differently than other nodes. They report whether their frame  has focus,
                // not whether focus is specifically on the root node.
                Focused = properties.GetValue("focused")?.ToObject<bool>() == true && this.role != "RootWebArea",
                Modal = properties.GetValue("modal")?.ToObject<bool>() ?? false,
                Multiline = properties.GetValue("multiline")?.ToObject<bool>() ?? false,
                Multiselectable = properties.GetValue("multiselectable")?.ToObject<bool>() ?? false,
                Readonly = properties.GetValue("readonly")?.ToObject<bool>() ?? false,
                Required = properties.GetValue("required")?.ToObject<bool>() ?? false,
                Selected = properties.GetValue("selected")?.ToObject<bool>() ?? false,
                Checked = this.GetCheckedState(properties.GetValue("checked")?.ToObject<string>()),
                Pressed = this.GetCheckedState(properties.GetValue("pressed")?.ToObject<string>()),
                Level = properties.GetValue("level")?.ToObject<int>() ?? 0,
                ValueMax = properties.GetValue("valuemax")?.ToObject<int>() ?? 0,
                ValueMin = properties.GetValue("valuemin")?.ToObject<int>() ?? 0,
                AutoComplete = this.GetIfNotFalse(properties.GetValue("autocomplete")?.ToObject<string>()),
                HasPopup = this.GetIfNotFalse(properties.GetValue("haspopup")?.ToObject<string>()),
                Invalid = this.GetIfNotFalse(properties.GetValue("invalid")?.ToObject<string>()),
                Orientation = this.GetIfNotFalse(properties.GetValue("orientation")?.ToObject<string>()),
            };

            return node;
        }

        private bool IsPlainTextField()
            => !this.richlyEditable && (this.editable || this.role == "textbox" || this.role == "ComboBox" || this.role == "searchbox");

        private bool IsTextOnlyObject()
            => this.role == "LineBreak" ||
                this.role == "text" ||
                this.role == "InlineTextBox" ||
                this.role == "StaticText";

        private bool HasFocusableChild()
        {
            return this.cachedHasFocusableChild ??= this.Children.Any(c => c.Focusable || c.HasFocusableChild());
        }

        private string GetIfNotFalse(string value) => value != null && value != "false" ? value : null;

        private CheckedState GetCheckedState(string value)
        {
            switch (value)
            {
                case "mixed":
                    return CheckedState.Mixed;
                case "true":
                    return CheckedState.True;
                default:
                    return CheckedState.False;
            }
        }
    }
}
