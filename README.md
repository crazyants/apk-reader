# apk-reader

read apk info (package name etc..) with out appt.

```csharp

            var reader = new ApkReader();
            var info = reader.Read(@"D:\tmp\wx.apk");
            Console.Clear();
            var json = JsonConvert.SerializeObject(info,new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            Console.WriteLine(json);
            Console.ReadLine();

```
