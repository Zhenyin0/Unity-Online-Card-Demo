# 联机经营卡牌Demo
客户端技术：Unity2022.3 C# UniTask WebSocket Protobuf DoTween
服务端技术: skynet_fly  mysql redis 
仅Windows平台客户端，实现大厅匹配、多场景切换、长连接通信、异步资源加载。
用途：个人学习项目，用于求职游戏客户端实习。
使用方法：Unity2022.3打开工程，自行填写服务端地址即可运行。
当前完成度: 仅实现功能，登录，注册，大厅场景的初始化，地图战斗的全部功能。

场景进入顺序: Load--->home───>map--->fight--->JieSuan
                    │        ^                  │
                    │        └----------------──┘
                    │
                    └─────> LianjiMingDan--->otherMap--->fight--->JieSuan
                                 │             ^                    │    
                                 │             └--------------------┘
                                 │              ┌─---------------------─┐
                                 │              ∨                       │
                                 └─-------─>selectmodule--->battle--->JieSuan

前端效果展示:
<img width="1920" height="1080" alt="MapSence1-fake" src="https://github.com/user-attachments/assets/d32e748c-9476-4d05-a7c3-151d07f4a7a9" />
<img width="1920" height="1080" alt="BattleSence-fake" src="https://github.com/user-attachments/assets/59eace75-3bc2-4bd0-921e-f8ea978485cb" />


后端架构展示:
<img width="4868" height="2189" alt="架构设计图1" src="https://github.com/user-attachments/assets/c143498f-8991-4d0a-8136-4b7e53680087" />





