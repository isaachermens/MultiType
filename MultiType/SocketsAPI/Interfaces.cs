using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiType.SocketsAPI
{
    public class StatsReceivedEventArgs : EventArgs
    {
        public UserStatistics StatsUpdate { get; set; }
    }

    public class ContentReceivedEventArgs : EventArgs
    {
        public string Content { get; set; }
    }

    public interface IGameController
    {
        Action<bool> SetPauseState { get; set; }
        Action<string, bool> NewLesson { get; set; }
        Action<bool> RepeatLesson { get; set; }
    }

    public class GameController : IGameController
    {
        public Action<bool> SetPauseState { get; set; }
        public Action<string, bool> NewLesson { get; set; }
        public Action<bool> RepeatLesson { get; set; }

        public GameController()
        {
            SetPauseState = delegate { };
            NewLesson = delegate { };
            RepeatLesson = delegate { };
        }
    }

    public interface IStatsNotify
    {
        event EventHandler<StatsReceivedEventArgs> StatsReceived;

        void OnStatsReceived(UserStatistics s);
    }

    public class StatsNotify : IStatsNotify
    {
        public event EventHandler<StatsReceivedEventArgs> StatsReceived = delegate { };
        public void OnStatsReceived(UserStatistics s)
        {
            StatsReceived(this, new StatsReceivedEventArgs { StatsUpdate = s });
        }
    }

    public interface IContentNotify
    {
        event EventHandler<ContentReceivedEventArgs> ContentReceived;

        void OnContentReceived(string content);
    }

    public class ContentNotify : IContentNotify
    {
        public event EventHandler<ContentReceivedEventArgs> ContentReceived = delegate { };
        public void OnContentReceived(string content)
        {
            ContentReceived(this, new ContentReceivedEventArgs { Content = content });
        }
    }

    public interface IPacketParser
    {
        void HandlePacket(SerializeBase packet);
    }

    public class MultiTypeParser : IPacketParser
    {
        private readonly IGameController _controllerInterface;
        private readonly IContentNotify _contentNotify;
        private readonly IStatsNotify _statsNotify;

        public MultiTypeParser(IGameController c, IStatsNotify sn, IContentNotify cn)
        {
            _controllerInterface = c;
            _contentNotify = cn;
            _statsNotify = sn;
        }

        public void HandlePacket(SerializeBase packet)
        {
            // todo can we replace the fields being checked with the is operator?
            if (packet.IsUserStatictics)
            { // use the data contained in the stats packet to update the peer databound properties in the view model
                var stats = (UserStatistics)packet;
                _statsNotify.OnStatsReceived(stats);
                _contentNotify.OnContentReceived(stats.TypedContent);
            }
            else if (packet.IsCommand)
            {
                var command = (Command)packet;
                //if (command.IsGameComplete) // alert the model that the game is complete
                //    // todo fix Model.GameIsComplete(false);
                //else if (command.IsPauseCommand)
                //{
                //    // todo fix Model.TogglePauseMulti(false);
                //    SetPauseState(command.GameHasStarted);
                //}
                //else if (command.StartCommand)
                //    // todo fix Model.StartGame(false, null); //command.StartTime, command.StopTime);
                //else if (command.IsResetCommand && command.ResetIsNewLesson)
                //{
                //    //_model.SendStatsPacket();
                //    // clear the lesson string and wait until the new lesson string is received from teh server
                //    NewLesson(command.LessonText, false);
                //}
                //else if (command.IsResetCommand && command.ResetIsRepeatedLesson)
                //{
                //    //_model.SendStatsPacket();
                //    RepeatLesson(false);
                //}
            }
        }
    }
}
