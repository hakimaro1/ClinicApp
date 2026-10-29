using ClinicApp.Data;
using ClinicApp.Models;
using ClinicApp.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicApp.Tests
{
    public class BookingTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Test_Booking_Taken_Slot_Throws_Exception()
        {
            // 1. ПОДГОТОВКА (Arrange)
            var context = GetInMemoryDbContext();
            var service = new AppointmentService(context);

            // Создаем тестовый слот (свободный)
            var slot = new TimeSlot { Id = 1, Start = DateTime.Now.AddDays(1), End = DateTime.Now.AddDays(1).AddMinutes(30), Status = "Free" };
            context.TimeSlots.Add(slot);
            await context.SaveChangesAsync();

            var patient1 = "patient1@test.ru";
            var patient2 = "patient2@test.ru";
            var doctorId = 1;

            // 2. ДЕЙСТВИЕ (Act)
            // Записываем первого пациента (должно пройти успешно)
            await service.BookAppointmentAsync(1, patient1, doctorId);

            // Пытаемся записать ВТОРОГО пациента на тот же слот
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await service.BookAppointmentAsync(1, patient2, doctorId));

            // 3. ПРОВЕРКА (Assert)
            Assert.Equal("Слот уже занят или не существует.", exception.Message);
        }
    }
}