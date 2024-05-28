namespace GrpcDemoService
{
    public class TicketRepository
    {
        private int _availableTickets = 5;
        public int GetAvailableTickets()
        {
            return _availableTickets;
        }

        public bool BuyTickets(string user, int count)
        {
            var updatedCount = _availableTickets - count;

            // Negative ticket count means there weren't enough available tickets
            if (updatedCount < 0)
            {
                return false;
            }

            _availableTickets = updatedCount;

            return true;
        }
    }
}
