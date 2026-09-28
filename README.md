# 联机经营卡牌Demo
客户端技术：Unity2022.3 C# UniTask native-WebSocket Newtonsoft.Json Protobuf DoTween

服务端技术: skynet_fly  mysql redis 

仅Windows平台客户端，实现大厅匹配、多场景切换、长连接通信、异步资源加载。

用途：个人学习项目，用于求职游戏服务端实习。

使用方法：Unity2022.3打开工程，自行填写服务端地址即可运行。

项目实现: • 游戏整体基于 UI 组件搭建，场景全部动效由 Dotween 库实现动画、物体形变、动画序列。核心开发内容为网
络模块，Unity 原生提供 http 报文请求能力，通过 native-websocket 建立长连接，借助 jsonNet 完成数据反
序列化还原，protobuf.dll 解析数据包，依托 UniTask 实现数据对象异步操作，遵循 rpc 规范完成客户端与服务
端交互。
• 服务端选用 skynet_fly 框架，相较于原生 skynet 架构完善度更高，内置数据库 ORM 调用封装、
pb.so-Protobuf 编解码封装、frpcpack.so-FRPC 数据包封装三层底层工具。 完整的底层封装统一开发规范，无
需重复基于 protos 协议文件编写 Lua 适配层，也无需基于 tcp、udp、kcp 等底层协议重复实现 rpc 通信规范。
基于该框架拆分业务逻辑，将通用调用逻辑抽入 commonluab 公共库，例如数据库调用函数可封装为 skynet_fly
内置 lualib 标准调用层。项目入口、配置、业务处理逻辑统一存放于 world games 分类文件。
• 调整 skynet_fly 上层调度逻辑，为每个游戏场景创建独立接待服务节点，通过 frpc 完成跨服务通信；各场景
内创建处理网络交互的业务对象，跨场景交互模块复用 native-websocket 通信通道。基于 protos 生成的协议
统一内层传输结构，protobuf.dll 处理二进制序列化信息，外层沿用 rpc 规范，再通过 jsonNet 序列化后对外发
送，统一规范网络业务通讯格式。
学历：本科
居住：永州
• 该游戏核心玩法为回合制经营策略，websocket 为主的 rpc 通信方案适配业务需求；后续如需开发高帧率联机同
步功能，可引入 kcp.so 底层 C 类库，并完成 Lua 层封装。

当前完成度: 仅实现功能，登录，注册，大厅场景的初始化，地图战斗的全部功能。

场景进入顺序: 
<img width="1005" height="384" alt="37AG6Q9V93(~T1Z`@$1@T0X" src="https://github.com/user-attachments/assets/71a78254-1ec2-4a70-b24b-140d29a9b131" />


前端效果展示:
<img width="1920" height="1080" alt="MapSence1-fake" src="https://github.com/user-attachments/assets/d32e748c-9476-4d05-a7c3-151d07f4a7a9" />
<img width="1920" height="1080" alt="BattleSence-fake" src="https://github.com/user-attachments/assets/59eace75-3bc2-4bd0-921e-f8ea978485cb" />


后端架构展示:
<img width="4868" height="2189" alt="架构设计图1" src="https://github.com/user-attachments/assets/c143498f-8991-4d0a-8136-4b7e53680087" />
<img width="2016" height="1267" alt="7UFGRSST``P$XG%RDUSJ4~H" src="https://github.com/user-attachments/assets/a3936baf-3abd-4fd2-a18b-148ecc15b093" />





