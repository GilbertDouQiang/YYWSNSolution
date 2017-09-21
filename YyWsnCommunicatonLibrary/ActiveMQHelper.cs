using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace YyWsnCommunicatonLibrary
{
    public class ActiveMQHelper
    {
        private IConnectionFactory factory;
        private IConnection connection;
        private ISession session;
        private IMessageProducer prod;
        private IMessageConsumer consumer;
        //private ITextMessage msg;
        private IBytesMessage msg;

        private bool isTopic = false;
        private bool hasSelector = false;
        //private const string ClientID = "clientid";
        private const string Selector = "filter='demo'";
        private bool sendSuccess = true;
        private bool receiveSuccess = true;

        public string ClientID { get; set; }
        public ActiveMQHelper(bool isLocalMachine, string remoteAddress,string clientID)
        {
            try
            {
                //初始化工厂   
                if (isLocalMachine)
                {
                    factory = new ConnectionFactory("tcp://localhost:61616?wireFormat.maxInactivityDuration=0");
                }
                else
                {
                    factory = new ConnectionFactory("tcp://" + remoteAddress + ":61616/"); //写tcp://192.168.1.111:61616的形式连接其他服务器上的ActiveMQ服务器           
                }
                //通过工厂建立连接
                connection = factory.CreateConnection();
                connection.ClientId = clientID;
                connection.Start();
                //通过连接创建Session会话
                session = connection.CreateSession();

                //temp

            }
            catch (System.Exception e)
            {
                sendSuccess = false;
                receiveSuccess = false;
                throw e;
            }

        }

        public bool InitQueueOrTopic(bool topic, string name, bool selector = false)
        {
            try
            {
                //通过会话创建生产者、消费者
                if (topic)
                {
                    prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(name));
                    if (selector)
                    {
                        consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(name), ClientID, Selector, false);
                        hasSelector = true;
                    }
                    else
                    {
                        consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(name), ClientID, null, false);
                        hasSelector = false;
                    }
                    isTopic = true;
                }
                else
                {
                    //标准的使用在这里
                    prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(name));
                    if (selector)
                    {
                        consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(name), Selector);
                        hasSelector = true;
                    }
                    else
                    {
                        //核心部分
                        consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(name));
                        hasSelector = false;
                    }
                    isTopic = false;
                }
                //创建一个发送的消息对象
                //msg = prod.CreateTextMessage();
                msg = prod.CreateBytesMessage();
            }
            catch (System.Exception e)
            {
                sendSuccess = false;
                receiveSuccess = false;
                throw e;
            }

            return sendSuccess;
        }

        public bool SendMessage(byte[] message, string msgId = "defult", MsgPriority priority = MsgPriority.Normal)
        {
            if (prod == null)
            {
                sendSuccess = false;
                Console.WriteLine("call InitQueueOrTopic() first!!");
                return false;
            }

            Console.WriteLine("Begin send messages...");

            //给这个对象赋实际的消息
            msg.NMSCorrelationID = msgId;
            msg.Properties["MyID"] = msgId;
            msg.NMSMessageId = msgId;
            msg.Content = message;

            
            //Console.WriteLine(message);

            if (isTopic)
            {
                sendSuccess = ProducerSubcriber(message, priority);
            }
            else
            {
                sendSuccess = P2P(message, priority);
            }

            return sendSuccess;
        }

        private bool ProducerSubcriber(byte[] message, MsgPriority priority)
        {
            try
            {
                prod.Priority = priority;
                //设置持久化,如果DeliveryMode没有设置或者设置为NON_PERSISTENT，那么重启MQ之后消息就会丢失
                prod.DeliveryMode = MsgDeliveryMode.Persistent;
                prod.Send(msg, Apache.NMS.MsgDeliveryMode.Persistent, priority, TimeSpan.MinValue);
                //prod.CreateBytesMessage()
                //IBytesMessage bytemessage = session.CreateBytesMessage();
                //bytemessage.writeBytes(content);

                //System.Threading.Thread.Sleep(1000);  
            }
            catch (System.Exception e)
            {
                sendSuccess = false;
              
                throw e;
            }

            return sendSuccess;
        }

        private bool P2P(byte[] message, MsgPriority priority)
        {
            try
            {
                if (hasSelector)
                {
                    //设置消息对象的属性，这个很重要，是Queue的过滤条件，也是P2P消息的唯一指定属性
                    msg.Properties.SetString("filter", "demo");  //P2P模式
                }
                prod.Priority = priority;
                //设置持久化
                prod.DeliveryMode = MsgDeliveryMode.Persistent;
                //生产者把消息发送出去，几个枚举参数MsgDeliveryMode是否持久化，MsgPriority消息优先级别，存活时间，当然还有其他重载
                prod.Send(msg, MsgDeliveryMode.Persistent, priority, TimeSpan.MinValue);
            }
            catch (System.Exception e)
            {
                sendSuccess = false;
               
                throw e;
            }

            return sendSuccess;
        }


        public void ShutDown()
        {
            try
            {
                session.Close();
                connection.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }


        public byte[] GetMessage()
        {
            if (prod == null)
            {
                //Console.WriteLine("call InitQueueOrTopic() first!!");
                return null;
            }

            //Console.WriteLine("Begin receive messages...");
            IBytesMessage revMessage = null;
            try
            {
                //同步阻塞10ms,没消息就直接返回null,注意此处时间不能设太短，否则还没取到消息就直接返回null了！！！
                revMessage = consumer.Receive(new TimeSpan(TimeSpan.TicksPerMillisecond * 10)) as IBytesMessage;
            }
            catch (System.Exception e)
            {
                receiveSuccess = false;
               
                throw e;
            }

            if (revMessage == null)
            {
                
                return null;
            }
            else
            {
                //Console.WriteLine("Received message with Correlation ID: " + revMessage.NMSCorrelationID);
                //Console.WriteLine("Received message with Properties'ID: " + revMessage.Properties["MyID"]);
                //Console.WriteLine("Received message with text: " + revMessage.Text);
            }

            return revMessage.Content;
        }


    }
}
