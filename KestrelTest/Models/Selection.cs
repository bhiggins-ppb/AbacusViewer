using System;

namespace AbacusViewer.Models
{
    public class Selection : IEquatable<Selection>
    {
        public long PaddyPowerId { get; set; }
        public string Name { get; set; }
        public bool? IsLine { get; set; }
        public decimal Probability { get; set; }
        public string MultiKey { get; set; }
        public int DisplayOrder { get; set; }
        public string SelectionIdentifier { get; set; }
        public int? PlayerParticipantId { get; set; }
        public string SelectionOrder { get; set; }
        public string SuspendAt { get; set; }

        #region EqualityMembers
        public bool Equals(Selection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PaddyPowerId == other.PaddyPowerId && string.Equals(Name, other.Name) && IsLine.Equals(other.IsLine) && Probability == other.Probability && string.Equals(MultiKey, other.MultiKey) && DisplayOrder == other.DisplayOrder && string.Equals(SelectionIdentifier, other.SelectionIdentifier) && PlayerParticipantId == other.PlayerParticipantId && string.Equals(SelectionOrder, other.SelectionOrder) && string.Equals(SuspendAt, other.SuspendAt);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Selection)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PaddyPowerId.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsLine.GetHashCode();
                hashCode = (hashCode * 397) ^ Probability.GetHashCode();
                hashCode = (hashCode * 397) ^ (MultiKey != null ? MultiKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DisplayOrder;
                hashCode = (hashCode * 397) ^ (SelectionIdentifier != null ? SelectionIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PlayerParticipantId.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectionOrder != null ? SelectionOrder.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SuspendAt != null ? SuspendAt.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Selection left, Selection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Selection left, Selection right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}