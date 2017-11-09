# aria2m ![icon](https://i.imgur.com/k5ifXRB.png)
### - A minimized aria2 tool to contact with aria2, aria2 webui and download links.

　

---
Features:
* Open/Close aria2 via trayicon.
* Open most of local aria2's webui via trayicon.
* Easily to add download links without open another aria2c (You need browser add-on like FlashGot).
* Save session when you close aria2 by aria2m or exit aria2m.
* Work fine with rpc-secret and rpc-listen-port. 

　

![screenshot](https://i.imgur.com/sZnFUQk.png)

　

---
The structure in your aria2m folder must like:
```
-aria2m
 -ui  <--folder of webui
    ...
    index.html
  aria2.conf
  aria2c.exe
  aria2m.exe
```
　

The arguments template in FlashGot (or other add-on) must be `[URL] [REFERER] [FOLDER]` or `[URL] [REFERER]`.
> There is a space between arguments.

　

---
Many thanks to [Peter Duniho](https://stackoverflow.com/questions/44876741/how-to-change-notifyicon-text-in-a-running-winform-via-command-line-arguments?noredirect=1#comment76771236_44876741).
> This is my first time to write C# program, if there are any stupid codes, please tell me, thank you very much!
