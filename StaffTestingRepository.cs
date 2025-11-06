using Moq;
using StaffAPI.Data;
using Xunit;

namespace StaffTesting;
public class StaffTestingRepository
{
    Mock<IEmployeeRepository> mock;

    StaffsContext staffsContext;

    IEmployeeRepository employeeRepository;

    public static List<Employee> expectedList { get; set; }

    public StaffTestingRepository()
    {
        mock = new Mock<IEmployeeRepository>();

        staffsContext = new StaffsContext();

        employeeRepository = new EmployeeRepository(staffsContext);

        expectedList = new List<Employee>();

        expectedList.Add(new Employee() { EmployeeId = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", DateofBirth = new DateTime(1990, 05, 15), GenderId = 1, DepartmentId = 1 });
        expectedList.Add(new Employee() { EmployeeId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", DateofBirth = new DateTime(1988, 11, 22), GenderId = 2, DepartmentId = 2 });
        expectedList.Add(new Employee() { EmployeeId = 3, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", DateofBirth = new DateTime(1995, 03, 30), GenderId = 2, DepartmentId = 1 });
        expectedList.Add(new Employee() { EmployeeId = 4, FirstName = "Bob", LastName = "Williams", Email = "bob.williams@example.com", DateofBirth = new DateTime(1985, 07, 08), GenderId = 1, DepartmentId = 3 });
    }

    [Fact]
    public async void GetStaff()
    {
        //Expected staff
        var employee = from emp in expectedList
                       where emp.EmployeeId == 2
                       select emp;
        var expectedEmployee = (employee == null) ? null : employee.FirstOrDefault();

        mock.Setup(x => x.GetEmployee(2)).ReturnsAsync(expectedEmployee);

        var result = await employeeRepository.GetEmployee(2);

        //Assertion 
        Assert.NotNull(result);
        Assert.Equal(expectedEmployee.EmployeeId, result.EmployeeId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]

    public async void GetStaffById(int id)
    {
        var employee = from emp in expectedList
                       where emp.EmployeeId == id
                       select emp;

        var expectedEmployee = (employee == null) ? null : employee.FirstOrDefault();

        if (expectedEmployee == null)
        {
            mock.Setup(x => x.GetEmployee(id)).ReturnsAsync(expectedEmployee);

            var result = await employeeRepository.GetEmployee(id);

            Assert.Null(result);
        }
        else
        {
            mock.Setup(x => x.GetEmployee(id)).ReturnsAsync(expectedEmployee);

            var result = await employeeRepository.GetEmployee(id);

            Assert.Equal(expectedEmployee.EmployeeId, result.EmployeeId);
        }
    }

    [Fact]
    public async void AddStaff_ValidStaff()
    {
        var newStaff = new Employee()
        {
            EmployeeId = 5,
            FirstName = "Michael",
            LastName = "Scott",
            Email = "michael.scott@example.com",
            DateofBirth = new DateTime(1971, 03, 15),
            GenderId = 1,
            DepartmentId = 2
        };

        mock.Setup(x => x.AddEmployee(newStaff)).ReturnsAsync(newStaff);

        var result = await mock.Object.AddEmployee(newStaff);

        Assert.Equal(newStaff.EmployeeId, result.EmployeeId);
    }

    public async void AddStaff_Invalid()
    {
        var invalidStaff = new Employee()
        {
            EmployeeId = 0,
            FirstName = "",
            LastName = "",
            Email = "",
            DateofBirth = new DateTime(1971, 03, 15),
            GenderId = 0,
            DepartmentId = 0
        };

        mock.Setup(x => x.AddEmployee(invalidStaff)).ReturnsAsync((Employee)null);
        var result = await mock.Object.AddEmployee(invalidStaff);
        Assert.Null(result);
    }

    [Fact]
    public async void AddStaff_ExistingId()
    {
        var newStaff = new Employee()
        {
            EmployeeId = 1,
            FirstName = "Duplicate",
            LastName = "User",
            Email = "",
            DateofBirth = new DateTime(1995, 5, 5),
            DepartmentId = 1,
            GenderId = 1,
        };
        var expectedStaff = newStaff;
        mock.Setup(x => x.AddEmployee(expectedStaff)).ReturnsAsync((Employee)null);
        var result = await mock.Object.AddEmployee(expectedStaff);
        Assert.Null(result);
    }

    [Fact]
    public async void GetAllStaffs_WithRecords()
    {

        mock.Setup(x => x.GetEmployees()).ReturnsAsync(expectedList);

        var result = await employeeRepository.GetEmployees();

        var resultList = result.ToList();

        Assert.Equal(expectedList.Count, result.Count());
        Assert.Equal(expectedList[0].EmployeeId, resultList[0].EmployeeId);
        Assert.Equal(expectedList[1].FirstName, resultList[1].FirstName);
    }

    [Fact]
    public async void GetAllStaffs_EmptyList()
    {
        var expectedStaffs = new List<Employee>();

        mock.Setup(x => x.GetEmployees()).ReturnsAsync(expectedStaffs);

        var result = await employeeRepository.GetEmployees();

        Assert.Empty(result);
    }

    [Fact]
    public async void UpdateStaff_ValidStaff()
    {
        var existingStaff = expectedList[0];
        var updatedStaff = new Employee { EmployeeId = 1, FirstName = "Edward", LastName = "Smith", Email = "edward.smith@example.com", DateofBirth = new DateTime(1990, 05, 15), GenderId = 1, DepartmentId = 1 };

        mock.Setup(x => x.GetEmployee(1)).ReturnsAsync(existingStaff);
        mock.Setup(x => x.UpdateEmployee(updatedStaff)).ReturnsAsync(updatedStaff);

        var result = await mock.Object.UpdateEmployee(updatedStaff);

        Assert.Equal(updatedStaff.FirstName, result.FirstName);
        Assert.Equal(updatedStaff.LastName, result.LastName);
    }


    [Fact]
    public async void UpdateStaff_StaffNotFound()
    {
        var nonExistingStaff = new Employee { EmployeeId = 999, FirstName = "Mark", LastName = "Taylor", Email = "mark.taylor@example.com", DateofBirth = new DateTime(1992, 06, 25), GenderId = 1, DepartmentId = 2 };

        mock.Setup(x => x.GetEmployee(999)).ReturnsAsync((Employee)null);
        mock.Setup(x => x.UpdateEmployee(nonExistingStaff)).ReturnsAsync((Employee)null);

        var result = await mock.Object.UpdateEmployee(nonExistingStaff);

        Assert.Null(result);
    }

    [Fact]
    public async void DeleteStaff_ValidStaff()
    {
        var staffToDelete = expectedList[0];

        mock.Setup(x => x.GetEmployee(staffToDelete.EmployeeId)).ReturnsAsync(staffToDelete);
        mock.Setup(x => x.DeleteEmployee(staffToDelete.EmployeeId)).ReturnsAsync(staffToDelete);

        var result = await mock.Object.DeleteEmployee(staffToDelete.EmployeeId);


        Assert.NotNull(result);
        Assert.Equal(staffToDelete.EmployeeId, result.EmployeeId);
        Assert.Equal(staffToDelete.FirstName, result.FirstName);
        Assert.Equal(staffToDelete.LastName, result.LastName);
    }

    [Fact]
    public async void DeleteEmployee_StaffNotFound()
    {
        var nonExistingEmployeeId = 999;

        mock.Setup(x => x.GetEmployee(nonExistingEmployeeId)).ReturnsAsync((Employee)null);
        mock.Setup(x => x.DeleteEmployee(nonExistingEmployeeId)).ReturnsAsync((Employee)null);

        var result = await mock.Object.DeleteEmployee(nonExistingEmployeeId);

        Assert.Null(result);
    }


}