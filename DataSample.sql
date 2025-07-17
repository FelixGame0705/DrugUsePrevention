-- Script tạo dữ liệu mẫu cho hệ thống phòng chống tệ nạn xã hội
-- Thứ tự insert theo dependency của foreign keys

-- 1. Insert Users (base table)
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserID, FullName, Username, Email, PasswordHash, Phone, AvatarUrl, DateOfBirth, Gender, Role, Status, CreatedAt, IsEmailVerified)
VALUES
(1, N'Nguyễn Văn Admin', 'admin', 'admin@drugprevention.vn', 'hashed_password_1', '0901234567', 'https://example.com/avatar1.jpg', '1985-05-15', 'Male', 'Admin', 'Active', '2024-01-01 08:00:00', 1),
(2, N'Trần Thị Hương', 'huongtran', 'huong.tran@drugprevention.vn', 'hashed_password_2', '0912345678', 'https://example.com/avatar2.jpg', '1990-08-20', 'Female', 'Consultant', 'Active', '2024-01-02 09:00:00', 1),
(3, N'Lê Minh Tuấn', 'tuanle', 'tuan.le@gmail.com', 'hashed_password_3', '0923456789', 'https://example.com/avatar3.jpg', '1995-12-10', 'Male', 'User', 'Active', '2024-01-03 10:00:00', 1),
(4, N'Phạm Thị Mai', 'maipham', 'mai.pham@yahoo.com', 'hashed_password_4', '0934567890', 'https://example.com/avatar4.jpg', '1992-03-25', 'Female', 'User', 'Active', '2024-01-04 11:00:00', 1),
(5, N'Hoàng Văn Nam', 'namhoang', 'nam.hoang@hotmail.com', 'hashed_password_5', '0945678901', 'https://example.com/avatar5.jpg', '1988-07-08', 'Male', 'Moderator', 'Active', '2024-01-05 12:00:00', 1);
SET IDENTITY_INSERT Users OFF;

-- 2. Insert Categories (base table)
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (CategoryID, CategoryName, CategoryDescription, ParentCategoryID, IsActive)
VALUES
(1, N'Giáo dục phòng chống', N'Các bài viết về giáo dục phòng chống tệ nạn xã hội', NULL, 1),
(2, N'Hỗ trợ tâm lý', N'Thông tin về hỗ trợ tâm lý và tư vấn', NULL, 1),
(3, N'Phục hồi và tái hòa nhập', N'Các chương trình phục hồi và tái hòa nhập xã hội', NULL, 1),
(4, N'Nghiên cứu khoa học', N'Các nghiên cứu khoa học về tệ nạn xã hội', NULL, 1),
(5, N'Tin tức sự kiện', N'Tin tức và sự kiện liên quan đến phòng chống tệ nạn', NULL, 1);
SET IDENTITY_INSERT Categories OFF;

-- 3. Insert Tags (base table)
SET IDENTITY_INSERT Tags ON;
INSERT INTO Tags (TagID, TagName, Note)
VALUES
(1, N'ma túy', N'Thẻ liên quan đến vấn đề ma túy'),
(2, N'giáo dục', N'Thẻ về giáo dục và tuyên truyền'),
(3, N'tư vấn', N'Thẻ về dịch vụ tư vấn'),
(4, N'phục hồi', N'Thẻ về quá trình phục hồi'),
(5, N'gia đình', N'Thẻ về vai trò của gia đình');
SET IDENTITY_INSERT Tags OFF;

-- 4. Insert Consultants (depends on Users)
-- No IDENTITY_INSERT needed for Consultants as ConsultantID is ForeignKey to UserID
INSERT INTO Consultants (ConsultantID, Qualifications, Specialty, WorkingHours)
VALUES
(2, N'Thạc sĩ Tâm lý học, Chứng chỉ tư vấn tâm lý', N'Tư vấn về nghiện chất', '["2024-07-15T08:00:00", "2024-07-15T14:00:00", "2024-07-16T08:00:00"]'),
(5, N'Tiến sĩ Y học, Chuyên khoa Tâm thần', N'Điều trị nghiện chất và rối loạn tâm thần', '["2024-07-15T09:00:00", "2024-07-15T15:00:00", "2024-07-17T09:00:00"]');

-- 5. Insert Courses (depends on Users)
SET IDENTITY_INSERT Courses ON;
INSERT INTO Courses (CourseID, Title, Description, TargetGroup, AgeGroup, ContentURL, ThumbnailURL, CreatedAt, isActive, isAccept, CreatedBy)
VALUES
(1, N'Tác hại của ma túy', N'Khóa học về tác hại của ma túy đối với sức khỏe và xã hội', N'Học sinh, sinh viên', N'15-25', 'https://example.com/course1', 'https://example.com/thumb1.jpg', '2024-01-10 08:00:00', 1, 1, 1),
(2, N'Kỹ năng từ chối ma túy', N'Hướng dẫn kỹ năng từ chối và tránh xa ma túy', N'Thanh thiếu niên', N'13-18', 'https://example.com/course2', 'https://example.com/thumb2.jpg', '2024-01-11 09:00:00', 1, 1, 2),
(3, N'Hỗ trợ gia đình', N'Cách gia đình hỗ trợ người nghiện phục hồi', N'Gia đình', N'Tất cả độ tuổi', 'https://example.com/course3', 'https://example.com/thumb3.jpg', '2024-01-12 10:00:00', 1, 1, 1),
(4, N'Phục hồi sau cai nghiện', N'Chương trình hỗ trợ phục hồi sau cai nghiện', N'Người từng nghiện', N'18+', 'https://example.com/course4', 'https://example.com/thumb4.jpg', '2024-01-13 11:00:00', 1, 1, 2),
(5, N'Tâm lý học nghiện', N'Hiểu biết về tâm lý của người nghiện', N'Chuyên gia', N'Người trưởng thành', 'https://example.com/course5', 'https://example.com/thumb5.jpg', '2024-01-14 12:00:00', 1, 1, 1);
SET IDENTITY_INSERT Courses OFF;

-- 6. Insert Programs (depends on Users)
SET IDENTITY_INSERT Programs ON;
INSERT INTO Programs (ProgramID, Title, Description, ThumbnailURL, StartDate, EndDate, Location, CreatedBy, IsActive, CreatorUserID)
VALUES
(1, N'Tuần lễ phòng chống tệ nạn xã hội 2024', N'Chương trình tuyên truyền phòng chống tệ nạn xã hội', 'https://example.com/program1.jpg', '2024-07-15 08:00:00', '2024-07-21 18:00:00', N'Hà Nội', 1, 1, 1),
(2, N'Hội thảo gia đình hạnh phúc', N'Hội thảo về vai trò gia đình trong phòng chống tệ nạn', 'https://example.com/program2.jpg', '2024-08-01 08:00:00', '2024-08-01 17:00:00', N'TP.HCM', 1, 1, 1),
(3, N'Trại hè thanh thiếu niên', N'Trại hè giáo dục kỹ năng sống cho thanh thiếu niên', 'https://example.com/program3.jpg', '2024-07-20 07:00:00', '2024-07-25 18:00:00', N'Đà Nẵng', 2, 1, 2),
(4, N'Tập huấn cho giáo viên', N'Chương trình tập huấn giáo dục phòng chống tệ nạn', 'https://example.com/program4.jpg', '2024-09-15 08:00:00', '2024-09-17 17:00:00', N'Cần Thơ', 1, 1, 1),
(5, N'Ngày hội sức khỏe cộng đồng', N'Ngày hội tuyên truyền sức khỏe và phòng chống tệ nạn', 'https://example.com/program5.jpg', '2024-10-01 07:00:00', '2024-10-01 18:00:00', N'Hải Phòng', 2, 1, 2);
SET IDENTITY_INSERT Programs OFF;

-- 7. Insert Surveys (base table)
SET IDENTITY_INSERT Surveys ON;
INSERT INTO Surveys (SurveyID, Title, Type, Description, CreatedAt, ThumbnailURL)
VALUES
(1, N'Đánh giá rủi ro ASSIST', 'ASSIST', N'Bảng câu hỏi đánh giá mức độ rủi ro sử dụng chất', '2024-01-01 08:00:00', 'https://example.com/survey1.jpg'),
(2, N'Sàng lọc CRAFFT cho thanh thiếu niên', 'CRAFFT', N'Bảng câu hỏi sàng lọc cho thanh thiếu niên', '2024-01-02 09:00:00', 'https://example.com/survey2.jpg'),
(3, N'Đánh giá trước chương trình', 'PreProgram', N'Đánh giá kiến thức trước khi tham gia chương trình', '2024-01-03 10:00:00', 'https://example.com/survey3.jpg'),
(4, N'Đánh giá sau chương trình', 'PostProgram', N'Đánh giá hiệu quả sau khi hoàn thành chương trình', '2024-01-04 11:00:00', 'https://example.com/survey4.jpg'),
(5, N'Khảo sát thái độ về ma túy', 'General', N'Khảo sát thái độ và nhận thức về tệ nạn ma túy', '2024-01-05 12:00:00', 'https://example.com/survey5.jpg');
SET IDENTITY_INSERT Surveys OFF;

-- 8. Insert NewsArticles (depends on Categories, Users)
SET IDENTITY_INSERT NewsArticles ON;
INSERT INTO NewsArticles (NewsArticleID, NewsAticleName, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID)
VALUES
(1, N'Tác hại ma túy mới', N'Phát hiện loại ma túy mới với tác hại khủng khiếp', '2024-07-01 08:00:00', N'Nội dung chi tiết về loại ma túy mới được phát hiện...', N'Báo An ninh Thủ đô', 1, N'Published', 1),
(2, N'Chương trình tư vấn miễn phí', N'Mở rộng chương trình tư vấn miễn phí cho người nghiện', '2024-07-02 09:00:00', N'Chi tiết về chương trình tư vấn miễn phí...', N'VTV1', 2, N'Published', 2),
(3, N'Thành công phục hồi', N'Câu chuyện thành công của người từng nghiện ma túy', '2024-07-03 10:00:00', N'Chia sẻ câu chuyện cảm động về quá trình phục hồi...', N'Báo Tuổi trẻ', 3, N'Published', 1),
(4, N'Nghiên cứu mới về nghiện', N'Nghiên cứu mới về cơ chế nghiện chất trong não bộ', '2024-07-04 11:00:00', N'Kết quả nghiên cứu khoa học mới nhất...', N'Tạp chí Y học', 4, N'Published', 1),
(5, N'Hội nghị quốc tế', N'Việt Nam tham gia hội nghị quốc tế về phòng chống ma túy', '2024-07-05 12:00:00', N'Thông tin về hội nghị quốc tế và vai trò của Việt Nam...', N'VOV', 5, N'Published', 2);
SET IDENTITY_INSERT NewsArticles OFF;

-- 9. Insert CourseContents (depends on Courses)
SET IDENTITY_INSERT CourseContents ON;
INSERT INTO CourseContents (ContentID, CourseID, Title, Description, ContentType, ContentData, OrderIndex, isActive, CreatedAt)
VALUES
(1, 1, N'Chương 1: Giới thiệu về ma túy', N'Khái niệm và phân loại ma túy', 'Video', 'https://example.com/video1.mp4', 1, 1, '2024-01-10 08:30:00'),
(2, 1, N'Chương 2: Tác hại sức khỏe', N'Tác hại của ma túy đối với sức khỏe', 'Video', 'https://example.com/video2.mp4', 2, 1, '2024-01-10 09:00:00'),
(3, 2, N'Bài 1: Nhận biết rủi ro', N'Cách nhận biết và tránh xa rủi ro', 'Text', '<p>Nội dung text về nhận biết rủi ro...</p>', 1, 1, '2024-01-11 09:30:00'),
(4, 2, N'Bài 2: Kỹ năng từ chối', N'Thực hành kỹ năng từ chối', 'Quiz', 'quiz_data_json', 2, 1, '2024-01-11 10:00:00'),
(5, 3, N'Phần 1: Hiểu về nghiện', N'Gia đình cần hiểu về nghiện chất', 'Video', 'https://example.com/video3.mp4', 1, 1, '2024-01-12 10:30:00');
SET IDENTITY_INSERT CourseContents OFF;

-- 10. Insert SurveyQuestions (depends on Surveys)
SET IDENTITY_INSERT SurveyQuestions ON;
INSERT INTO SurveyQuestions (QuestionID, SurveyID, QuestionText, QuestionType)
VALUES
(1, 1, N'Trong 3 tháng qua, bạn có sử dụng rượu bia không?', 'SingleChoice'),
(2, 1, N'Trong 3 tháng qua, bạn có sử dụng thuốc lá không?', 'SingleChoice'),
(3, 2, N'Bạn có bao giờ đi xe trong khi đã uống rượu không?', 'SingleChoice'),
(4, 3, N'Kiến thức của bạn về tác hại ma túy như thế nào?', 'MultipleChoice'),
(5, 5, N'Theo bạn, nguyên nhân chính khiến người ta sử dụng ma túy là gì?', 'Text');
SET IDENTITY_INSERT SurveyQuestions OFF;

-- 11. Insert SurveyAnswers (depends on SurveyQuestions)
SET IDENTITY_INSERT SurveyAnswers ON;
INSERT INTO SurveyAnswers (AnswerID, QuestionID, AnswerText, IsCorrect)
VALUES
(1, 1, N'Không bao giờ', 0),
(2, 1, N'1-2 lần/tháng', 0),
(3, 1, N'Hàng tuần', 1),
(4, 2, N'Không', 0),
(5, 2, N'Có', 1),
(6, 3, N'Không bao giờ', 0),
(7, 3, N'Có', 1),
(8, 4, N'Rất hiểu rõ', 1),
(9, 4, N'Hiểu một phần', 0),
(10, 4, N'Không hiểu', 0);
SET IDENTITY_INSERT SurveyAnswers OFF;

-- 12. Insert CourseRegistrations (depends on Users, Courses)
SET IDENTITY_INSERT CourseRegistrations ON;
INSERT INTO CourseRegistrations (RegistrationID, UserID, CourseID, RegisteredAt, Completed)
VALUES
(1, 3, 1, '2024-01-15 08:00:00', 0),
(2, 4, 1, '2024-01-15 09:00:00', 1),
(3, 3, 2, '2024-01-16 10:00:00', 0),
(4, 4, 3, '2024-01-17 11:00:00', 0),
(5, 5, 1, '2024-01-18 12:00:00', 1);
SET IDENTITY_INSERT CourseRegistrations OFF;

-- 13. Insert CheckCourseContents (depends on CourseRegistrations, CourseContents)
SET IDENTITY_INSERT CheckCourseContents ON;
INSERT INTO CheckCourseContents (CheckID, RegistrationID, ContentID, IsCompleted, CompletedAt)
VALUES
(1, 1, 1, 1, '2024-01-15 09:00:00'),
(2, 1, 2, 0, NULL),
(3, 2, 1, 1, '2024-01-15 10:00:00'),
(4, 2, 2, 1, '2024-01-15 11:00:00'),
(5, 3, 3, 1, '2024-01-16 11:00:00');
SET IDENTITY_INSERT CheckCourseContents OFF;

-- 14. Insert UserSurveyResponses (depends on Users, Surveys)
SET IDENTITY_INSERT UserSurveyResponses ON;
INSERT INTO UserSurveyResponses (ResponseID, UserID, SurveyID, CompletedAt, RiskLevel, Recommendation)
VALUES
(1, 3, 1, '2024-01-20 08:00:00', 'Low', N'Duy trì lối sống lành mạnh'),
(2, 4, 1, '2024-01-20 09:00:00', 'Moderate', N'Cần tư vấn thêm về rủi ro'),
(3, 3, 2, '2024-01-21 10:00:00', 'Low', N'Tiếp tục giáo dục phòng ngừa'),
(4, 4, 3, '2024-01-22 11:00:00', 'High', N'Cần can thiệp ngay lập tức'),
(5, 5, 5, '2024-01-23 12:00:00', 'Moderate', N'Tăng cường giáo dục');
SET IDENTITY_INSERT UserSurveyResponses OFF;

-- 15. Insert UserSurveyAnswers (depends on UserSurveyResponses, SurveyQuestions, SurveyAnswers)
SET IDENTITY_INSERT UserSurveyAnswers ON;
INSERT INTO UserSurveyAnswers (UserSurveyAnswerID, ResponseID, QuestionID, SelectedAnswerID)
VALUES
(1, 1, 1, 1),
(2, 1, 2, 4),
(3, 2, 1, 3),
(4, 2, 2, 5),
(5, 3, 3, 6);
SET IDENTITY_INSERT UserSurveyAnswers OFF;

-- 16. Insert Appointments (depends on Users, Consultants)
SET IDENTITY_INSERT Appointments ON;
INSERT INTO Appointments (AppointmentID, UserID, ConsultantID, ScheduledAt, Status, Notes, CreatedAt)
VALUES
(1, 3, 2, '2024-07-20 09:00:00', 'Confirmed', N'Tư vấn lần đầu về nghiện game', '2024-07-15 08:00:00'),
(2, 4, 2, '2024-07-21 10:00:00', 'Pending', N'Tư vấn cho gia đình', '2024-07-15 09:00:00'),
(3, 3, 5, '2024-07-22 14:00:00', 'Completed', N'Đánh giá và điều trị', '2024-07-15 10:00:00'),
(4, 4, 5, '2024-07-23 15:00:00', 'Cancelled', N'Người dùng hủy lịch', '2024-07-15 11:00:00'),
(5, 5, 2, '2024-07-24 11:00:00', 'Confirmed', N'Tư vấn tâm lý', '2024-07-15 12:00:00');
SET IDENTITY_INSERT Appointments OFF;

-- 17. Insert ProgramParticipations (depends on Users, Programs)
SET IDENTITY_INSERT ProgramParticipations ON;
INSERT INTO ProgramParticipations (ParticipationID, UserID, ProgramID, ParticipatedAt)
VALUES
(1, 3, 1, '2024-07-15 08:30:00'),
(2, 4, 1, '2024-07-15 09:00:00'),
(3, 5, 2, '2024-08-01 08:30:00'),
(4, 3, 3, '2024-07-20 07:30:00'),
(5, 4, 5, '2024-10-01 07:30:00');
SET IDENTITY_INSERT ProgramParticipations OFF;

-- 18. Insert NewsTags (depends on NewsArticles, Tags)
SET IDENTITY_INSERT NewsTags ON;
INSERT INTO NewsTags (NewsTagID, NewsArticleID, TagID)
VALUES
(1, 1, 1),
(2, 1, 2),
(3, 2, 3),
(4, 3, 4),
(5, 4, 1),
(6, 5, 2),
(7, 2, 5),
(8, 3, 5),
(9, 4, 2),
(10, 5, 1);
SET IDENTITY_INSERT NewsTags OFF;

-- 19. Insert DashboardData (base table)
SET IDENTITY_INSERT DashboardData ON;
INSERT INTO DashboardData (ID, Metric, Value, UpdatedAt)
VALUES
(1, 'TotalUsers', 150, '2024-07-13 08:00:00'),
(2, 'ActiveCourses', 25, '2024-07-13 08:00:00'),
(3, 'CompletedAppointments', 89, '2024-07-13 08:00:00'),
(4, 'OngoingPrograms', 12, '2024-07-13 08:00:00'),
(5, 'SurveyResponses', 267, '2024-07-13 08:00:00');
SET IDENTITY_INSERT DashboardData OFF;

-- Success message
PRINT 'Sample data inserted successfully for Drug Use Prevention Database!';