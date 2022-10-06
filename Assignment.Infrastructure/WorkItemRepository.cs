using Assignment.Core;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int WorkItemId) Create(WorkItemCreateDTO workItem)
    {
        var entity = _context.WorkItems.FirstOrDefault(w => w.Title == workItem.Title);
        Response response;

        if (entity is null)
        {
            var user = _context.Users.Find(workItem.AssignedToId);

            if (user is null && workItem.AssignedToId is not null) 
            { 
                return (Response.BadRequest, (int)workItem.AssignedToId); 
            }
            else
            {
                var tags = _context.Tags.Where(t => workItem.Tags.Contains(t.Name)).ToArray();
                entity = new WorkItem
                {
                    Title = workItem.Title,
                    AssignedTo = user,
                    AssignedToId = user!.Id,
                    Description = workItem.Description,
                    State = State.New,
                    Tags = tags,
                    Created = DateTime.UtcNow,
                    StateUpdated = DateTime.UtcNow
                };

                _context.WorkItems.Add(entity);
                _context.SaveChanges();
                response = Response.Created;
            }
        }
        else response = Response.Conflict;

        return (response, entity.Id);
    }

    public Response Delete(int workItemId)
    {
        var entity = _context.WorkItems.Find(workItemId);

        if (entity is null) return Response.NotFound;
        else if (entity.State == State.New)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return Response.Deleted;
        }
        else if (entity.State == State.Active)
        {
            entity.State = State.Removed;
            return Response.Deleted;
        }

        return Response.Conflict;
    }

    public WorkItemDetailsDTO Find(int workItemId)
    {
        var w = _context.WorkItems.Find(workItemId);
        
        if (w is not null)
        {
            var u = _context.Users.FirstOrDefault(u => u.Id == w!.AssignedTo!.Id);
            var name = u is null ? "" : u.Name;
            var tags = w.Tags.Select(t => t.Name).ToArray();
            return new WorkItemDetailsDTO(w.Id, w.Title, w.Description!, w.Created, name, tags, (Core.State)w.State, w.StateUpdated);
        }

        return null!;
    }

    public IReadOnlyCollection<WorkItemDTO> Read() => StandardRead(_context.WorkItems);

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state) =>
        StandardRead(_context.WorkItems.Where(w => w.State == state));

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag) =>
        StandardRead(_context.WorkItems.Where(w => w.Tags.Select(t => t.Name).Contains(tag)));
    
    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId) =>
        StandardRead(_context.WorkItems.Where(w => w.AssignedTo!.Id == userId));
    
    public IReadOnlyCollection<WorkItemDTO> ReadRemoved() => ReadByState(State.Removed);

    private IReadOnlyCollection<WorkItemDTO> StandardRead(IQueryable<WorkItem> workItems) =>
        (from w in workItems
        let u = _context.Users.FirstOrDefault(u => u.Id == w!.AssignedTo!.Id)
        orderby w.Title
        select new WorkItemDTO
        (
            w.Id,
            w.Title,
            u == null ? "" : u.Name,
            w.Tags.Select(t => t.Name).ToArray(),
            w.State
        )).ToArray();

    public Response Update(WorkItemUpdateDTO workItem)
    {
        var entity = _context.WorkItems.Find(workItem.Id);
        if (entity is not null)
        {
            var user = _context.Users.Find(workItem.AssignedToId);
            if (user is null) return Response.BadRequest;
            
            entity.Id = workItem.Id;
            entity.Title = workItem.Title;
            entity.AssignedTo = user;
            entity.Description = workItem.Description;
            var tags = _context.Tags.Where(t => workItem.Tags.Contains(t.Name)).ToArray();
            entity.Tags = tags;
            if (entity.State != workItem.State)
            {
                entity.StateUpdated = DateTime.UtcNow;
            }
            entity.State = workItem.State;
            _context.SaveChanges();
            return Response.Updated;
        }

        return Response.NotFound;
    }
}
