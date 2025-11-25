// <copyright file="SerializedAXNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageAccessibility
{
    using System;
    using System.Linq;

    /// <summary>
    /// AXNode.
    /// </summary>
    public class SerializedAXNode : IEquatable<SerializedAXNode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedAXNode"/> class.
        /// </summary>
        public SerializedAXNode() => this.Children = Array.Empty<SerializedAXNode>();

        /// <summary>
        /// Gets or sets the <see fref="https://www.w3.org/TR/wai-aria/#usage_intro">role</see>.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets a human readable name for the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current value of the node.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets an additional human readable description of the node.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets keyboard shortcuts associated with this node.
        /// </summary>
        public string KeyShortcuts { get; set; }

        /// <summary>
        /// Gets or sets a human readable alternative to the role.
        /// </summary>
        public string RoleDescription { get; set; }

        /// <summary>
        /// Gets or sets a description of the current value.
        /// </summary>
        public string ValueText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is expanded or collapsed.
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is focused.
        /// </summary>
        public bool Focused { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is <see href="https://en.wikipedia.org/wiki/Modal_window">modal</see>.
        /// </summary>
        public bool Modal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node text input supports multiline.
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether more than one child can be selected.
        /// </summary>
        public bool Multiselectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is read only.
        /// </summary>
        public bool Readonly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the node is selected in its parent node.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets whether the checkbox is checked, or "mixed".
        /// </summary>
        public CheckedState Checked { get; set; }

        /// <summary>
        /// Gets or sets whether the toggle button is checked, or "mixed".
        /// </summary>
        public CheckedState Pressed { get; set; }

        /// <summary>
        /// Gets or sets the level of a heading.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in a node.
        /// </summary>
        public int ValueMin { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in a node.
        /// </summary>
        public int ValueMax { get; set; }

        /// <summary>
        /// Gets or sets what kind of autocomplete is supported by a control.
        /// </summary>
        public string AutoComplete { get; set; }

        /// <summary>
        /// Gets or sets what kind of popup is currently being shown for a node.
        /// </summary>
        public string HasPopup { get; set; }

        /// <summary>
        /// Gets or sets whether and in what way this node's value is invalid.
        /// </summary>
        public string Invalid { get; set; }

        /// <summary>
        /// Gets or sets whether the node is oriented horizontally or vertically.
        /// </summary>
        public string Orientation { get; set; }

        /// <summary>
        /// Gets or sets child nodes of this node, if any.
        /// </summary>
        public SerializedAXNode[] Children { get; set; }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Exceptions should not be raised in this type of method.")]
        public bool Equals(SerializedAXNode other)
            => ReferenceEquals(this, other) ||
                (
                    this.Role == other.Role &&
                    this.Name == other.Name &&
                    this.Value == other.Value &&
                    this.Description == other.Description &&
                    this.KeyShortcuts == other.KeyShortcuts &&
                    this.RoleDescription == other.RoleDescription &&
                    this.ValueText == other.ValueText &&
                    this.AutoComplete == other.AutoComplete &&
                    this.HasPopup == other.HasPopup &&
                    this.Orientation == other.Orientation &&
                    this.Disabled == other.Disabled &&
                    this.Expanded == other.Expanded &&
                    this.Focused == other.Focused &&
                    this.Modal == other.Modal &&
                    this.Multiline == other.Multiline &&
                    this.Multiselectable == other.Multiselectable &&
                    this.Readonly == other.Readonly &&
                    this.Required == other.Required &&
                    this.Selected == other.Selected &&
                    this.Checked == other.Checked &&
                    this.Pressed == other.Pressed &&
                    this.Level == other.Level &&
                    this.ValueMin == other.ValueMin &&
                    this.ValueMax == other.ValueMax &&
                    (this.Children == other.Children || this.Children.SequenceEqual(other.Children)));

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is SerializedAXNode s && this.Equals(s);

        /// <inheritdoc/>
        public override int GetHashCode()
            => this.Role.GetHashCode() ^
                this.Name.GetHashCode() ^
                this.Value.GetHashCode() ^
                this.Description.GetHashCode() ^
                this.KeyShortcuts.GetHashCode() ^
                this.RoleDescription.GetHashCode() ^
                this.ValueText.GetHashCode() ^
                this.AutoComplete.GetHashCode() ^
                this.HasPopup.GetHashCode() ^
                this.Orientation.GetHashCode() ^
                this.Disabled.GetHashCode() ^
                this.Expanded.GetHashCode() ^
                this.Focused.GetHashCode() ^
                this.Modal.GetHashCode() ^
                this.Multiline.GetHashCode() ^
                this.Multiselectable.GetHashCode() ^
                this.Readonly.GetHashCode() ^
                this.Required.GetHashCode() ^
                this.Selected.GetHashCode() ^
                this.Pressed.GetHashCode() ^
                this.Checked.GetHashCode() ^
                this.Level.GetHashCode() ^
                this.ValueMin.GetHashCode() ^
                this.ValueMax.GetHashCode() ^
                this.Children.GetHashCode();
    }
}
