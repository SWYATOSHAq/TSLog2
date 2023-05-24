using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Yard.BO;

namespace TSLog.Yard.Test
{
    [TestFixture]
    internal class YardControllerTests
    {
        [Test]
        public void CreateOperationByPartner_StartDateParsing_Success()
        {
            // Arrange
            //var configurationMock = new Mock<IConfiguration>();
            //var storageMock = new Mock<IYardStorage>();
            //var rightsCheckerMock = new Mock<RightsChecker>();

            //configurationMock.Setup(c => c.GetValue<bool>("UseAuthentication")).Returns(true);
            //var yardControllerTest = new YardController(configurationMock, storageMock, rightsCheckerMock); // Replace "YourClass" with the actual class name


            //rightsCheckerMock.Setup(rc => rc.CheckRight("USE_YARD", It.IsAny<User>())).Returns(true);
            //rightsCheckerMock.Setup(rc => rc.CheckRight("CREATE_OPERATION_BY_PARTNER", It.IsAny<User>())).Returns(true);
            // Mock other necessary methods and properties

            // Set up the mocked dependencies in the class instance
            //yardControllerTest._rightsChecker = rightsCheckerMock.Object;
            //yardControllerTest._storage = storageMock.Object;
            // Set up other necessary dependencies in the class instance

            // Act
            //var result = yardControllerTest.CreateOperationByPartner().GetAwaiter().GetResult();

            // Assert
            //Assert.IsFalse(result.StartsWith("Wrong start date"));
        }
    }
}
