# UnityProject(Unity Mirror를 이용한 실시간 전략게임(RTS) 제작)


이 프로젝트에서는 Unity 3D 엔진과 오픈소스인 Mirror를 이용하여 실시간 전략게임(Real-Time Strategy, RTS)을 다룬다.

Unity 엔진으 이용하면 무료로 하나의 환경에서 쉽게 그래픽, 물리, 네트워크, 사운드, 스크립트, 애니메이션, Asset관리, 장면 관리 등을 할 수 있으며 개발 비용과 시간을 단축시킬 수 있으며 개인 프로젝트에 적합하다.

Mirror는 Unity에서 사용되는 네트워크의 고수준 API와 네트워크 라이브러리를 지원하는 오픈소스이며, 이를 이용하면 서버 엔진을 구현하는 시간과 비용을 단축하며 멀티플레이 환경을 구현 할 수 있다.

Unity 3D URP 기반으로 제작.

<p align="center">
 
> <a href="https://ko.wikipedia.org/wiki/%EB%B9%84%EB%94%94%EC%98%A4_%EA%B2%8C%EC%9E%84_%EC%9E%A5%EB%A5%B4">Genre: Real-Time Strategy(RTS)</a>
 
> <a href="https://unity3d.com/get-unity/download/archive">Unity 2021.2.7f1</a>

> Platform: Win32/64
 
> “This project is licensed under the terms of the MIT license.”
</p>

### Game Design
> User Experience: 전술적, 사고적, 실시간

> Theme: Post-apocalypse

> Model: Low poly 
> ![image](https://user-images.githubusercontent.com/41105616/177569054-7569bc2a-d093-413c-a61b-be1b11552a12.png)


### Network Concept
![image](https://user-images.githubusercontent.com/41105616/177563734-529ee4eb-3501-4fdb-8837-a408178ce796.png)
* 1 vs 1

* Peer-to-Peer Network

* Using FizzySteamworks Transport in Mirror
