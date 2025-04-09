﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using JetBrains.Annotations;

namespace Nice3point.Revit.Toolkit.Options;

/// <summary>
///     Configuration for creating <see cref="ISelectionFilter" /> instances.
/// </summary>
/// <example>
///     <code>
///         var selectionConfiguration = new SelectionConfiguration()
///             .Allow.Element(element => element.Category.Id.AreEquals(BuiltInCategory.OST_Walls))
///             .Allow.Reference((reference, xyz) => false);
///             
///         uiDocument.Selection.PickObject(ObjectType.Element, selectionConfiguration.Filter);
///     </code>
/// </example>
[PublicAPI]
public class SelectionConfiguration
{
    private readonly SelectionFilterInternal _filter;

    /// <summary>
    ///     Construct a <see cref="SelectionConfiguration" />
    /// </summary>
    public SelectionConfiguration()
    {
        _filter = new SelectionFilterInternal(this);
    }

    /// <summary>
    ///     Object filter for selection operation.
    /// </summary>
    public ISelectionFilter Filter => _filter;

    /// <summary>
    ///     Configures allowed filters for object selection.
    /// </summary>
    public ISelectionFilterConfiguration Allow => _filter;

    private class SelectionFilterInternal(SelectionConfiguration selectionConfiguration) : ISelectionFilter, ISelectionFilterConfiguration
    {
        private Func<Element, bool>? _elementHandler;
        private Func<Reference, XYZ, bool>? _referenceHandler;

        public bool AllowElement(Element elem)
        {
            return _elementHandler == null || _elementHandler.Invoke(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return _referenceHandler == null || _referenceHandler.Invoke(reference, position);
        }

        public SelectionConfiguration Element(Func<Element, bool> elementHandler)
        {
            _elementHandler = elementHandler;
            return selectionConfiguration;
        }

        public SelectionConfiguration Reference(Func<Reference, XYZ, bool> referenceHandler)
        {
            _referenceHandler = referenceHandler;
            return selectionConfiguration;
        }
    }
}

/// <summary>
///     An interface that provides the ability to filter objects during a selection operation.
/// </summary>
[PublicAPI]
public interface ISelectionFilterConfiguration
{
    /// <summary>
    ///     Handler indicating if a reference to a piece of geometry should be permitted to be selected.
    /// </summary>
    /// <param name="elementHandler">
    ///     Selection handler.<br />
    ///     Element – A candidate element in selection operation.
    /// </param>
    /// <returns>Return true to allow the user to select this candidate element. Return false to prevent selection of this element.</returns>
    /// <remarks>
    ///     If prompting the user to select an element from a Revit Link instance, the element passed here will be the link instance, not the selected linked element.<br />
    ///     If an exception is thrown from this method, the element will not be permitted to be selected.
    /// </remarks>
    public SelectionConfiguration Element(Func<Element, bool> elementHandler);

    /// <summary>
    ///     Handler indicating if the element should be permitted to be selected.
    /// </summary>
    /// <param name="referenceHandler">
    ///     Selection handler.<br />
    ///     Reference – A candidate reference in selection operation.<br />
    ///     position – The 3D position of the mouse on the candidate reference.
    /// </param>
    /// <returns>
    ///     Return true to allow the user to select this candidate reference. Return false to prevent selection of this candidate.<br />
    ///     If an exception is thrown from this method, the element will not be permitted to be selected.
    /// </returns>
    public SelectionConfiguration Reference(Func<Reference, XYZ, bool> referenceHandler);
}