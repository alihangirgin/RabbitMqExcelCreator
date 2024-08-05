Bu solution, kullanıcıların web arayüzü üzerinden RabbitMQ kullanarak Excel dosyalarının oluşturulma akışının başlatılmasınıı ve bu dosyaların arka planda işlenip hazır olduğunda kullanıcıya bildirilmesini içermektedir.

![image](https://github.com/user-attachments/assets/5cbd0195-1a96-480b-9d3a-51700d7dbad0)


**Genel Akış**

- Kullanıcı bir Excel dosyası oluşturma talebinde bulunur.
  
  ![image](https://github.com/user-attachments/assets/f5870768-e1e6-4849-9bd7-ae4ccfb3b03c)

- ProductController bir mesajı RabbitMQ'ya gönderir.

UserFile entity'si oluşturulur ve RabbitMQClient bu mesajı publish eder. UserFile Entity oluşturma amacı Excel oluşturma akışını kayıt altına almaktır.

![image](https://github.com/user-attachments/assets/23fc7d96-7cd7-44bf-b19c-b5aee497e668)

- İlgili Queue'yu dinleyen bir Worker Service bulunur. Worker Service mesajı tüketirek ClosedXML kütüpanesi ile veritabanından bir Excel dosyası oluşturur. Oluşturulan dosya IFormFile olarak ana uygulamadaki Upload isteğine gönderilir

![image](https://github.com/user-attachments/assets/923aa18f-443d-4988-adfb-c698191ce743)

- Upload isteği gelen IFormFile'ı karşılar ve wwwRoot'da static file olarak Excel dosyasını oluşturur ve UserFile Entity'yi günceller. SignalR ile Client'a dosyanın hazır olduğunu bildirir.

![image](https://github.com/user-attachments/assets/6ec0e46b-d120-4792-b487-f1d8a89977d3)

- Oluşturulan dosyalar UserFile Entity'leri ile listelenir.

  ![image](https://github.com/user-attachments/assets/a1db8e0d-4eaf-4e9b-806f-222e73c238b8)

- Belirli bir yönlendirme anahtarı ile (Routing Key) eşleşen kuyruğa gönderilmesini sağlayan RabbitMQ Direct Exchange modeli kullanılmıştır.

![image](https://github.com/user-attachments/assets/667cb61f-31dd-4430-900c-e5347ac48546)
