using Assignment.Core;
namespace Assignment.Infrastructure;

public class UserRepository
{

    private readonly KanbanContext _context;
    public UserRepository(KanbanContext context){
        this._context = context;
    }
    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var entity = _context.Users.FirstOrDefault(user => user.Name == user.Name);
        Response response;

        if(entity == null)
        {
            entity = new User();
            entity.Name = user.Name;

            _context.Users.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }

        return (response, entity.Id);
    }

    public UserDTO Find(int userId){
         var user = from u in _context.Users
                where u.Id == userId
                select new UserDTO(u.Id, u.Name, u.Email);

        return user.First();   
    }

    public IReadOnlyCollection<UserDTO> Read(){
        var users = from u in _context.Users
                    orderby u.Name
                    select new UserDTO(u.Id, u.Name, u.Email);

            return users.ToArray();
    }

    public Response Update(UserUpdateDTO user){
        var entity = _context.Users.Find(user.Id);

        Response response;

        if(entity == null){
            response = Response.NotFound;
        } else if (_context.Tags.FirstOrDefault(u => u.Id != user.Id && u.Name == user.Name) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = user.Name;
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;
    }

    public Response Delete(int userId, bool force = false){
         var user = _context.Users.FirstOrDefault(user => user.Id == userId);
        Response response;  
        if(user == null){
            response = Response.NotFound;
        } else if (user.WorkItems != null && user.WorkItems.Any()){
            if(force){
            response = Response.Deleted;
            _context.Users.Remove(user); //deletes from fatabase
            } 
            else
            {
                response = Response.Conflict;
            }
        } 
        else
        {
            response = Response.Deleted;
             _context.Users.Remove(user); //deletes from fatabase
        }
        return response;
    }
}
