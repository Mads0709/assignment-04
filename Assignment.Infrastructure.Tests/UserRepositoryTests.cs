namespace Assignment.Infrastructure.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Assignment.Core;



public class UserRepositoryTests
{

    private readonly KanbanContext _context;
	private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        List<User> users = new()
        {
            new User{Id = 4, Name = "Jens", Email = "jens@gmail.com"},
            new User{Id = 5, Name = "Bo", Email = "bo@gmail.com"}
        };


        List<Tag> tags = new()
        {
            new Tag{Id = 4, Name = "Smart"},
            new Tag{Id = 5, Name = "Green"}

        };

        List<WorkItem> workItems = new()
        {
            //new WorkItem{Id = 4, State = State.Active, Title = "Project", AssignedTo = users[1], AssignedToId = 2},
            new WorkItem{Id = 5, State = State.New, Title = "Milestone", AssignedTo = users[0], AssignedToId = 1},
            new WorkItem{Id = 6, State = State.Removed, Title = "Task"}
        };

        context.Users.AddRange(users);
        context.Tags.AddRange(tags);
        context.WorkItems.AddRange(workItems);

        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
    }

    [Fact]

    public void CreateGivenUser(){
        //Arrange
        var (Response, UserId) = _repository.Create(new UserCreateDTO("ITU", "abe@itu.dk"));

        //Assert
        Response.Should().Be(Response.Created);

        UserId.Should().Be(2);

    }    

    [Fact]

    public void DeleteUser(){
        //Arrange
        var deletedUser = _repository.Delete(5); //takes Jens from the database with id 11

        //Act
        deletedUser.Should().Be(Response.Deleted);

    }

    [Fact]
    public void FindTag(){
        //Arrange
        var findTag = _repository.Find(5);

        //Act
        findTag.Id.Should().Be(5);
        findTag.Name.Should().Be("Bo");
    }

    public void ReadUsers(){

        UserDTO[] array = {new UserDTO(4, "Jens", "jens@gmail.com"), new UserDTO(5,"Bo", "Bo@gmail.com")}; 
        //Arrange
        var readTag = _repository.Read();

        //Act
        readTag.Should().BeEquivalentTo(array);
    }

   
    [Fact]
    public void UpdateUser(){
        //Arrange
        var user = _repository.Update(new UserUpdateDTO(5, "NewName", "mail@mail.dk"));

        //Act
        user.Should().Be(Response.Updated);
    }
}