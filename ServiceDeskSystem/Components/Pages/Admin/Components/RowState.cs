namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Which row of an admin table is open: at most one, either in edit or in the delete prompt.
/// The new-record row counts as an edit row.
/// </summary>
public sealed class RowState
{
    private int? editId;
    private int? deleteId;

    /// <summary>
    /// Gets a value indicating whether the empty row for a new record is open.
    /// </summary>
    public bool IsAdding { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a save or delete is running.
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Gets the error of the last failed save, shown in the edit row.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Gets a value indicating whether an edit row (existing or new record) is open.
    /// </summary>
    public bool IsEditOpen => this.IsAdding || this.editId is not null;

    /// <summary>
    /// Returns whether the row of record <paramref name="id"/> is in edit.
    /// </summary>
    /// <param name="id">The record id.</param>
    /// <returns><c>true</c> when the record is being edited.</returns>
    public bool IsEditing(int id) => this.editId == id;

    /// <summary>
    /// Returns whether the row of record <paramref name="id"/> shows the delete prompt.
    /// </summary>
    /// <param name="id">The record id.</param>
    /// <returns><c>true</c> when the record awaits delete confirmation.</returns>
    public bool IsDeleting(int id) => this.deleteId == id;

    /// <summary>
    /// Opens the edit row of record <paramref name="id"/>, closing any other open row.
    /// </summary>
    /// <param name="id">The record id.</param>
    public void Edit(int id)
    {
        this.Close();
        this.editId = id;
    }

    /// <summary>
    /// Opens the empty row for a new record, closing any other open row.
    /// </summary>
    public void Add()
    {
        this.Close();
        this.IsAdding = true;
    }

    /// <summary>
    /// Opens the delete prompt of record <paramref name="id"/>, closing any other open row.
    /// </summary>
    /// <param name="id">The record id.</param>
    public void ConfirmDelete(int id)
    {
        this.Close();
        this.deleteId = id;
    }

    /// <summary>
    /// Closes the open row and drops its error.
    /// </summary>
    public void Close()
    {
        this.editId = null;
        this.deleteId = null;
        this.IsAdding = false;
        this.Error = null;
        this.IsBusy = false;
    }

    /// <summary>
    /// Marks the start of a save or delete.
    /// </summary>
    public void Begin()
    {
        this.IsBusy = true;
        this.Error = null;
    }

    /// <summary>
    /// Keeps the row open and shows <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The message to show.</param>
    public void Fail(string error)
    {
        this.IsBusy = false;
        this.Error = error;
    }
}
