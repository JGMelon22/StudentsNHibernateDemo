using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using NHibernateDemo.Core.Domains.Entities;
using NHibernateDemo.Infrastructure.Interfaces.Repositories;

namespace NHibernateDemo.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<StudentRepository> _logger;

    public StudentRepository(ISessionFactory sessionFactory, ILogger<StudentRepository> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    public async Task<bool> AddStudentAsync(Student student)
    {
        _logger.LogInformation("Adding a new student: {Student}", student);

        using ISession session = _sessionFactory.OpenSession();
        using ITransaction transaction = session.BeginTransaction();

        try
        {
            await session.SaveAsync(student);
            await transaction.CommitAsync();

            _logger.LogInformation("Student added successfully with ID: {StudentId}", student.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add student: {Student}", student);
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<Student?> GetStudentAsync(int id)
    {
        _logger.LogInformation("Fetching student with ID: {StudentId}", id);

        using ISession session = _sessionFactory.OpenSession();

        try
        {
            Student? student = await session.GetAsync<Student>(id);

            if (student is not null)
                _logger.LogInformation("Student retrieved: {Student}", student);

            return student;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with ID: {StudentId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Student>> GetStudentsListAsync()
    {
        _logger.LogInformation("Fetching list of all students...");

        using ISession session = _sessionFactory.OpenSession();

        try
        {
            IEnumerable<Student> students = await session.Query<Student>().ToListAsync();
            _logger.LogInformation("Retrieved {Count} students.", students.Count());
            return students;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student list.");
            return [];
        }
    }

    public async Task<bool> RemoveStudentAsync(int id)
    {
        _logger.LogInformation("Removing student with ID: {StudentId}", id);

        using ISession session = _sessionFactory.OpenSession();
        using ITransaction transaction = session.BeginTransaction();

        try
        {
            Student student = await session.GetAsync<Student>(id);
            if (student is null)
                return false;

            await session.DeleteAsync(student);
            await transaction.CommitAsync();

            _logger.LogInformation("Student with ID {StudentId} removed successfully.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing student with ID: {StudentId}", id);
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UpdateStudentAsync(int id, Student updatedStudent)
    {
        _logger.LogInformation("Updating student with ID: {StudentId}", id);

        using ISession session = _sessionFactory.OpenSession();
        using ITransaction transaction = session.BeginTransaction();

        try
        {
            Student student = await session.GetAsync<Student>(id);
            if (student is null)
                return false;

            student.Name = updatedStudent.Name;
            student.Course = updatedStudent.Course;
            student.Gender = updatedStudent.Gender;

            await session.UpdateAsync(student);
            await transaction.CommitAsync();

            _logger.LogInformation("Student with ID {StudentId} updated successfully.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student with ID: {StudentId}", id);
            await transaction.RollbackAsync();
            return false;
        }
    }
}
