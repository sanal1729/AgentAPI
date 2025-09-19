// <copyright file="Area.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Entities
{
    public class Area
    {
        // Private field for children areas
        private readonly List<Area> _areaNodes = new();

        public Area(long id, string name, string code, long? parentId = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Code = code ?? throw new ArgumentNullException(nameof(code));
            ParentId = parentId;
        }

        protected Area()
        {
        }

        // Primary Key
        public long Id { get; private set; }

        // Area name
        public string Name { get; private set; } = null!;

        // Short identifier/code
        public string Code { get; private set; } = null!;

        // Parent navigation and foreign key
        public long? ParentId { get; private set; }

        public Area? Parent { get; private set; }

        // Children collection exposed as read-only
        public IReadOnlyCollection<Area> AreaNodes => _areaNodes.AsReadOnly();

        // Path for path enumeration (e.g. "/1/4/10/")
        public string Path { get; private set; } = string.Empty;

        // Level (depth) in the hierarchy, root = 0
        public int Level { get; private set; }

        // Add child node
        public void AddAreaNode(Area areaNode)
        {
            if (areaNode == null)
            {
                throw new ArgumentNullException(nameof(areaNode));
            }

            _areaNodes.Add(areaNode);
        }

        // Set path and level based on parent path and level
        public void SetPathAndLevel(string parentPath, int parentLevel)
        {
            Path = string.IsNullOrEmpty(parentPath) ? $"/{Id}/" : $"{parentPath}{Id}/";
            Level = parentLevel + 1;
        }

        // Rename area
        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            }

            Name = name;
        }

        // Change code
        public void Changecode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("code cannot be empty.", nameof(code));
            }

            Code = code;
        }
    }
}
