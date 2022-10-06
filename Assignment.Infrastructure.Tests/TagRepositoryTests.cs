using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Assignment.Core;

namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests : IDisposable
{
	private readonly KanbanContext _context;
	private readonly TagRepository _repository;

	public TagRepositoryTests()
	{
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);

        var context = new KanbanContext(builder.Options);
        context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
       
         List<User> users = new()
        {
            new User{Id = 1, Name = "Jens", Email = "jens@gmail.com"},
            new User{Id = 2, Name = "Bo", Email = "bo@gmail.com"}
        };

        List<Tag> tags = new()
        {
            new Tag{Id = 1, Name = "Smart"},
            new Tag{Id = 2, Name = "Green"}
        };

        List<WorkItem> workItems = new()
        {
            new WorkItem{Id = 1, State = State.Active, Title = "Project"},
            new WorkItem{Id = 2, State = State.New, Title = "Milestone"},
            new WorkItem{Id = 3, State = State.Removed, Title = "Task"}
        };

        context.Users.AddRange(users);
        context.Tags.AddRange(tags);
        context.WorkItems.AddRange(workItems);

        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

	public void Dispose()
	{
        _context.Database.EnsureDeleted();
        _context.Dispose();

	}


    [Fact]

    public void CreateGivenTag(){
        //Arrange
        var (Response, TagId) = _repository.Create(new TagCreateDTO("ITU"));

        //Assert
        Response.Should().Be(Response.Created);

        TagId.Should().Be(3);

    }    

    [Fact]

    public void DeleteTag(){
        //Arrange
        var deletedTag = _repository.Delete(1); //takes Jens from the database with id 11

        //Act
        deletedTag.Should().Be(Response.Deleted);

    }

    [Fact]
    public void FindTag(){
        //Arrange
        var findTag = _repository.Find(2);

        //Act
        findTag.Id.Should().Be(2);
        findTag.Name.Should().Be("Green");
    }

     [Fact]
    public void ReadTags(){

        TagDTO[] array = {new TagDTO(2, "Green"), new TagDTO(1,"Smart")}; 
        //Arrange
        var readTag = _repository.Read();

        //Act
        readTag.Should().BeEquivalentTo(array);
    }

    [Fact]
    public void UpdateTag(){
        //Arrange
        var tag = _repository.Update(new TagUpdateDTO(1, "NewName"));

        //Act
        tag.Should().Be(Response.Updated);
    }
}
