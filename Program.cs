using System;
using System.Collections.Generic;
using System.Linq;


public class Booking
{
    public int Id { get; set; }
    public string RoomNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventName { get; set; }
    public string Organizer { get; set; }
    public string Description { get; set; }
}

public class RoomAvailabilityRequest
{
    public string RoomNumber { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class RoomBookingModule
{
    private readonly List<Booking> _bookings;
    private int _nextId;

    public RoomBookingModule()
    {
        _bookings = new List<Booking>();
        _nextId = 1;
    }

    public BookingResult BookRoom(string roomNumber, DateTime startTime, DateTime endTime,
                                string eventName, string organizer, string description = "")
    {
     
        if (startTime >= endTime)
            return BookingResult.Failure("Время окончания должно быть позже времени начала");

        if (startTime.Date != endTime.Date)
            return BookingResult.Failure("Бронирование должно быть в пределах одного дня");

        
        var conflictingBooking = _bookings.FirstOrDefault(b =>
            b.RoomNumber == roomNumber &&
            b.StartTime < endTime &&
            b.EndTime > startTime);

        if (conflictingBooking != null)
        {
            return BookingResult.Failure(
                $"Аудитория {roomNumber} уже занята с {conflictingBooking.StartTime:HH:mm} до {conflictingBooking.EndTime:HH:mm}");
        }

        var booking = new Booking
        {
            Id = _nextId++,
            RoomNumber = roomNumber,
            StartTime = startTime,
            EndTime = endTime,
            EventName = eventName,
            Organizer = organizer,
            Description = description
        };

        _bookings.Add(booking);
        return BookingResult.success(booking);
    }


    public List<Booking> GetRoomOccupancy(DateTime date)
    {
        return _bookings
            .Where(b => b.StartTime.Date == date.Date)
            .OrderBy(b => b.RoomNumber)
            .ThenBy(b => b.StartTime)
            .ToList();
    }

   
    public List<Booking> GetRoomOccupancy(DateTime startTime, DateTime endTime)
    {
        return _bookings
            .Where(b => b.StartTime < endTime && b.EndTime > startTime)
            .OrderBy(b => b.RoomNumber)
            .ThenBy(b => b.StartTime)
            .ToList();
    }

   
    public AvailabilityCheckResult CheckRoomAvailability(string roomNumber, DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            return AvailabilityCheckResult.InvalidTime();

        var conflictingBooking = _bookings.FirstOrDefault(b =>
            b.RoomNumber == roomNumber &&
            b.StartTime < endTime &&
            b.EndTime > startTime);

        if (conflictingBooking != null)
        {
            return AvailabilityCheckResult.Occupied(conflictingBooking);
        }

        return AvailabilityCheckResult.Available();
    }


    public AvailabilityCheckResult CheckRoomAvailability(RoomAvailabilityRequest request)
    {
        var startDateTime = request.Date.Date.Add(request.StartTime);
        var endDateTime = request.Date.Date.Add(request.EndTime);

        return CheckRoomAvailability(request.RoomNumber, startDateTime, endDateTime);
    }


    public List<Booking> GetRoomBookings(string roomNumber)
    {
        return _bookings
            .Where(b => b.RoomNumber == roomNumber)
            .OrderBy(b => b.StartTime)
            .ToList();
    }


    public bool CancelBooking(int bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null)
        {
            _bookings.Remove(booking);
            return true;
        }
        return false;
    }

    public List<Booking> GetAllBookings()
    {
        return _bookings
            .OrderBy(b => b.StartTime)
            .ThenBy(b => b.RoomNumber)
            .ToList();
    }

   
    public void ClearAllBookings()
    {
        _bookings.Clear();
        _nextId = 1;
    }
}


public class BookingResult
{
    public bool Success { get; set; }
    public Booking Booking { get; set; }
    public string ErrorMessage { get; set; }

    public static BookingResult success(Booking booking)
    {
        return new BookingResult
        {
            
            Booking = booking
        };
    }

    public static BookingResult Failure(string error) => new BookingResult
    {
        Success = false,
        ErrorMessage = error
    };
}

public class AvailabilityCheckResult
{
    public bool IsAvailable { get; set; }
    public bool IsOccupied => !IsAvailable;
    public Booking ConflictingBooking { get; set; }
    public string Message { get; set; }

    public static AvailabilityCheckResult Available() => new AvailabilityCheckResult
    {
        IsAvailable = true,
        Message = "Аудитория свободна"
    };

    public static AvailabilityCheckResult Occupied(Booking booking) => new AvailabilityCheckResult
    {
        IsAvailable = false,
        ConflictingBooking = booking,
        Message = $"Аудитория занята: {booking.EventName} ({booking.Organizer})"
    };

    public static AvailabilityCheckResult InvalidTime() => new AvailabilityCheckResult
    {
        IsAvailable = false,
        Message = "Некорректное время бронирования"
    };
}


public class Program
{
    public static void Main()
    {
        var bookingModule = new RoomBookingModule();


        var result1 = bookingModule.BookRoom(
            "101",
            new DateTime(2024, 1, 15, 10, 0, 0),
            new DateTime(2024, 1, 15, 12, 0, 0),
            "Лекция по математике",
            "Иванов И.И."
        );

        var result2 = bookingModule.BookRoom(
            "102",
            new DateTime(2024, 1, 15, 11, 0, 0),
            new DateTime(2024, 1, 15, 13, 0, 0),
            "Семинар по программированию",
            "Петров П.П."
        );


        var occupancy = bookingModule.GetRoomOccupancy(new DateTime(2024, 1, 15));
        Console.WriteLine("Занятость аудиторий 15.01.2024:");
        foreach (var booking in occupancy)
        {
            Console.WriteLine($"{booking.RoomNumber}: {booking.StartTime:HH:mm}-{booking.EndTime:HH:mm} - {booking.EventName} ({booking.Organizer})");
        }


        var check = bookingModule.CheckRoomAvailability(
            "101",
            new DateTime(2024, 1, 15, 11, 0, 0),
            new DateTime(2024, 1, 15, 12, 0, 0)
        );

        Console.WriteLine($"\nПроверка аудитории 101: {check.Message}");


        var request = new RoomAvailabilityRequest
        {
            RoomNumber = "102",
            Date = new DateTime(2024, 1, 15),
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(15, 0, 0)
        };

        var check2 = bookingModule.CheckRoomAvailability(request);
        Console.WriteLine($"Проверка аудитории 102: {check2.Message}");
    }
}