using System.Collections.Immutable;
using System.Linq;

namespace TofuEngine;

public class CollectionWithSelection<T>
{
    public IReadOnlyList<T> Items // { get; private set; }
        = ArraySegment<T>.Empty;

    private List<int> _selectedIndexes = new List<int>();

    public void SelectFirst()
    {
        SelectItems([0]);
    }

    public void SelectItems(IReadOnlyList<int>? indexes)
    {
        indexes = indexes ?? ArraySegment<int>.Empty;
        _selectedIndexes.Clear();
        _selectedIndexes.AddRange(indexes);
    }

    public IReadOnlyList<T> GetSelectedItems()
    {
        // idkkkkkkkkkkkkkkk
        return _selectedIndexes.Select(index => Items[index]).ToList();
    }

    public T GetFirstSelectedItem()
    {
        return Items[_selectedIndexes.FirstOrDefault(defaultValue: 0)];
    }

    public IReadOnlyList<int> GetSelectedIndices() => _selectedIndexes;
}