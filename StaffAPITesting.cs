using Moq;
using Microsoft.AspNetCore.Mvc;
using StaffAPI.Controllers;
using StaffAPI.Models;
using StaffAPI.Repositories;
using Org.BouncyCastle.Asn1.Misc;

namespace StaffTesting
{
    public class StaffAPITesting
    {
        private readonly Mock<IStaffRepository> mockRepo;
        private readonly StaffController controller;

        public StaffAPITesting()
        {
            mockRepo = new Mock<IStaffRepository>();
            controller = new StaffController(mockRepo.Object);
        }

        [Fact]
        public async Task GetStaffs_ReturnsOk_WithListOfStaffs()
        {
            // Arrange
            var expectedList = new List<Staff>
            {
                new Staff { StaffId = 1, StaffName = "La Tri Tam", Email = "tamla@example.com", PhoneNumber = "+1234567890", StartingDate = DateTime.Now },
                new Staff { StaffId = 2, StaffName = "Nguyen Minh Nghi", Email = "nghinguyen@example.com", PhoneNumber = "+9876543210", StartingDate = DateTime.Now },
                new Staff { StaffId = 3, StaffName = "Nguyen Thanh Sang", Email = "sangnguyen@example.com", PhoneNumber = "+84 123456789", StartingDate = DateTime.Now }
            };

            mockRepo.Setup(r => r.GetStaffs()).ReturnsAsync(expectedList);

            // Act
            var result = await controller.GetStaffs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualList = Assert.IsAssignableFrom<IEnumerable<Staff>>(okResult.Value);
            Assert.Equal(expectedList.Count, ((List<Staff>)actualList).Count);
        }

        [Theory]
        [InlineData(1, "La Tri Tam", "tamla@example.com", "0123456789")]
        [InlineData(2, "Nguyen Minh Nghi", "nghinguyen@example.com", "0987654321")]
        [InlineData(3, "Nguyen Thanh Sang", "sangnguyen@example.com", "0123456789")]
        public async Task GetStaff_WhenFound(int id, string name, string email, string phone)
        {
            // Arrange
            var staff = new Staff { StaffId = id, StaffName = name, Email = email, PhoneNumber = phone, StartingDate = DateTime.Now };
            mockRepo.Setup(r => r.GetStaff(id)).ReturnsAsync(staff);

            // Act
            var result = await controller.GetStaff(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStaff = Assert.IsType<Staff>(okResult.Value);
            Assert.Equal(id, returnedStaff.StaffId);
        }

        [Theory]
        [InlineData(99)]
        [InlineData(-99)]
        [InlineData(999999999)]
        public async Task GetStaff_WhenMissing(int id)
        {
            // Arrange
            mockRepo.Setup(r => r.GetStaff(id)).ReturnsAsync((Staff)null!);

            // Act
            var result = await controller.GetStaff(id);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value!.ToString());
        }

        [Theory]
        [InlineData(1, "Alice Nguyen", "alice@example.com", "+84 123456789")]
        [InlineData(2, "Bob Tran", "bob.tran@company.vn", "+84 912345678")]
        [InlineData(3, "Charlie Pham", "charlie.pham@work.co", "0905123456")]
        [InlineData(4, "Daisy Le", "daisy.le@office.org", "+1-202-555-0188")]
        [InlineData(5, "Ethan Vo", "ethan.vo@domain.io", "+123 987654321")]
        public async Task CreateStaff_WhenValid(int id, string name, string email, string phone)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = id,
                StaffName = name,
                Email = email,
                PhoneNumber = phone,
                StartingDate = DateTime.Now
            };

            mockRepo.Setup(r => r.AddStaff(It.IsAny<Staff>())).ReturnsAsync(staff);

            // Act
            var result = await controller.CreateStaff(staff);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdStaff = Assert.IsType<Staff>(created.Value);

            Assert.Equal(name, createdStaff.StaffName);
            Assert.Equal(email, createdStaff.Email);
            Assert.Equal(phone, createdStaff.PhoneNumber);
        }

        [Theory]
        [InlineData(1, "Alice Nguyen", "alice@", "0123456789")]
        [InlineData(2, "Bob Tran", "bob", "0123456789")]
        [InlineData(3, "Charlie Pham", "charlie.pham@@work.co", "0123456789")]
        [InlineData(4, "Daisy Le", "daisy_le@.org", "0123456789")]
        [InlineData(5, "Ethan Vo", "ethan.vo@domain..io", "0123456789")]
        public async Task CreateStaff_WhenInvalidEmail(int id, string name, string email, string phone)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = id,
                StaffName = name,
                Email = email,
                PhoneNumber = phone,
                StartingDate = DateTime.Now
            };

            controller.ModelState.AddModelError("Email", "Invalid email format.");

            // Act
            var result = await controller.CreateStaff(staff);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        [Theory]
        [InlineData(1, "Alice Nguyen", "alice@example.com", "123")]
        [InlineData(2, "Bob Tran", "bob@company.vn", "abcdefghij")]
        [InlineData(3, "Charlie Pham", "charlie@work.co", "0905-abc-123")]
        [InlineData(4, "Daisy Le", "daisy@office.org", "++84912345678")]
        [InlineData(5, "Ethan Vo", "ethan@domain.io", "01234567890123456789")]
        public async Task CreateStaff_WhenInvalidPhoneNumber(int id, string name, string email, string phone)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = id,
                StaffName = name,
                Email = email,
                PhoneNumber = phone,
                StartingDate = DateTime.Now
            };

            controller.ModelState.AddModelError("PhoneNumber", "Invalid phone number format.");

            // Act
            var result = await controller.CreateStaff(staff);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        [Fact]
        public async Task UpdateStaff_ReturnsOk_WhenUpdated()
        {
            // Arrange
            var staff = new Staff { StaffId = 1, StaffName = "La Tri Tam", Email = "tamla@gmail.com", PhoneNumber = "0123456789", StartingDate = DateTime.Now };
            mockRepo.Setup(r => r.GetStaff(1)).ReturnsAsync(staff);
            mockRepo.Setup(r => r.UpdateStaff(It.IsAny<Staff>())).ReturnsAsync(staff);

            // Act
            var result = await controller.UpdateStaff(1, staff);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<Staff>(okResult.Value);
        }

        [Fact]
        public async Task UpdateStaff_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = 2,
                StaffName = "La Tri Tam",
                Email = "tamla@gmail.com",
                PhoneNumber = "0123456789",
                StartingDate = DateTime.Now
            };

            // Act
            var result = await controller.UpdateStaff(1, staff);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Staff ID mismatch.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateStaff_ReturnsNotFound_WhenStaffDoesNotExist()
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = 1,
                StaffName = "La Tri Tam",
                Email = "tamla@gmail.com",
                PhoneNumber = "0123456789",
                StartingDate = DateTime.Now
            };

            mockRepo.Setup(r => r.GetStaff(1)).ReturnsAsync((Staff)null!);

            // Act
            var result = await controller.UpdateStaff(1, staff);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Staff with Id = 1 not found.", notFound.Value);
        }

        [Fact]
        public async Task DeleteStaff_ReturnsOk_WhenDeleted()
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = 1,
                StaffName = "Nguyen Minh Nghi",
                Email = "nghinguyen@example.com",
                PhoneNumber = "0123456789",
                StartingDate = DateTime.Now
            };

            mockRepo.Setup(r => r.GetStaff(1)).ReturnsAsync(staff);
            mockRepo.Setup(r => r.DeleteStaff(1)).ReturnsAsync(staff);

            // Act
            var result = await controller.DeleteStaff(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var deletedStaff = Assert.IsType<Staff>(okResult.Value);
            Assert.Equal("Nguyen Minh Nghi", deletedStaff.StaffName);
        }

        [Fact]
        public async Task DeleteStaff_ReturnsNotFound_WhenStaffDoesNotExist()
        {
            // Arrange
            mockRepo.Setup(r => r.GetStaff(99)).ReturnsAsync((Staff)null!);

            // Act
            var result = await controller.DeleteStaff(99);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Staff with Id = 99 not found.", notFound.Value);
        }

        [Fact]
        public async Task DeleteStaff_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            mockRepo.Setup(r => r.GetStaff(1)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.DeleteStaff(1);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error deleting staff record.", objectResult.Value);
        }
    }
}
