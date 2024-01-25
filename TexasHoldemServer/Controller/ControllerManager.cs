using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using TexasHoldemProtobuf;
using TexasHoldemServer.Servers;

namespace TexasHoldemServer.Controller
{
    class ControllerManager
    {
        protected Dictionary<RequestCode, BaseController> controllDic = new Dictionary<RequestCode, BaseController>();
        private Server server;

        public ControllerManager(Server server)
        {
            this.server = server;

            UserController userController = new UserController();
            controllDic.Add(userController.GetRequestCode, userController);
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack">解析後的消息</param>
        public void HandleRequest(MainPack pack, Client client)
        {
            if (controllDic.TryGetValue(pack.RequestCode, out BaseController controller))
            {
                string metname = pack.ActionCode.ToString();
                MethodInfo method = controller.GetType().GetMethod(metname);
                if (method == null)
                {
                    Console.WriteLine("沒有找到對應的處理方法:" + metname);
                    return;
                }

                object[] objs = new object[] { server, client, pack };
                object ret = method.Invoke(controller, objs);
                if (ret != null)
                {
                    client.Send(ret as MainPack);
                }
            }
            else
            {
                Console.WriteLine("沒有找到對應的 controller 處理");
            }
        }
    }
}
