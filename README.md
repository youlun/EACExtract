# 使用方式
0. 把 eac3to 放在环境变量(%path%)里
1. 拖入 .m2ts
2. 点击按钮，默认抽取以下几种轨道
3. <pre>h264/AVC             -> .h264<br>MPEGH/ISO/HEVC      -> .hevc<br>RAW/PCM             -> .flac<br>DTS Master Audio    -> .flac<br>FLAC                -> .flac<br>AAC                 -> .m4a<br>DTS                 -> .dts<br>TrueHD/AC3          -> .thd<br>AC3                 -> .ac3<br>EAC3                -> .eac3<br>Subtitle (PGS)      -> .sup<br>Subtitle (SRT)      -> .srt</pre>
4. 等着
5. 一旦发现不认识的轨道类型，会弹出对话框，请上报
6. 完成后，抽取的轨道会保存在 .m2ts 相同的文件夹中
<pre>00000.m2ts
00000.2 - Log.txt
00000.2.flac
00000.3.flac
00000.4.dts
00000.5.sup
00000.6.txt</pre>
