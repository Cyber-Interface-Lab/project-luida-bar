using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class GiftInfo
    {
        static readonly string hex = "0123456789abcdef";
        static readonly string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public readonly string giftType;
        public readonly string id;
        public readonly EmulateVector3 initialPosition;
        public readonly EmulateQuaternion initialRotation;
        public readonly EmulateVector3 initialVelocity;
        public readonly double price;
        public readonly PlayerHandle sender;
        public readonly string senderDisplayName;
        public readonly double timestamp;

        public GiftInfo(
            string giftType,
            EmulateVector3 initialPosition,
            EmulateQuaternion initialRotation,
            double price,
            PlayerHandle sender
        )
        {
            this.giftType = giftType;
            this.id = String.Format("{0}_{1}", RandomString(alphabet, 4), RandomString(hex, 26));
            this.initialPosition = initialPosition;
            this.initialRotation = initialRotation;
            this.initialVelocity = new EmulateVector3(0, 0, 10).applyQuaternion(initialRotation);
            this.price = price;
            this.sender = sender;
            this.senderDisplayName = sender.userDisplayName;
            this.timestamp = UnixEpoch();

        }

        string RandomString(string source, int length)
        {
            var sb = new StringBuilder(length);
            var random = new Random();
            for(int i = 0; i < length; i++)
            {
                sb.Append(source[random.Next(0, source.Length)]);
            }
            return sb.ToString();
        }

        double UnixEpoch()
        {
            var epoch = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);
            return (long)epoch.TotalSeconds;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[GiftInfo][{0}][{1}][{2}][{3}]", id, timestamp, giftType, price);
        }
    }
}
