namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Sort state of an admin table: the active column and its direction.
/// </summary>
public sealed class TableSort
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableSort"/> class.
    /// </summary>
    /// <param name="column">The column sorted first.</param>
    /// <param name="descending">Whether that column starts in descending order.</param>
    public TableSort(string column, bool descending = false)
    {
        this.Column = column;
        this.Descending = descending;
    }

    /// <summary>
    /// Gets the active column.
    /// </summary>
    public string Column { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the active column is sorted in descending order.
    /// </summary>
    public bool Descending { get; private set; }

    /// <summary>
    /// Sorts by <paramref name="column"/>; the active column flips its direction, a new one starts ascending
    /// (or descending when <paramref name="descendingFirst"/> is set, e.g. for counts).
    /// </summary>
    /// <param name="column">The column header that was clicked.</param>
    /// <param name="descendingFirst">Whether a newly selected column starts in descending order.</param>
    public void Toggle(string column, bool descendingFirst = false)
    {
        if (this.Column == column)
        {
            this.Descending = !this.Descending;
            return;
        }

        this.Column = column;
        this.Descending = descendingFirst;
    }

    /// <summary>
    /// Orders <paramref name="items"/> by <paramref name="key"/> in the current direction.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <typeparam name="TKey">The sort key type.</typeparam>
    /// <param name="items">The rows.</param>
    /// <param name="key">The key of the active column.</param>
    /// <param name="comparer">An optional key comparer.</param>
    /// <returns>The ordered rows.</returns>
    public IOrderedEnumerable<T> Apply<T, TKey>(IEnumerable<T> items, Func<T, TKey> key, IComparer<TKey>? comparer = null) =>
        this.Descending ? items.OrderByDescending(key, comparer) : items.OrderBy(key, comparer);
}
