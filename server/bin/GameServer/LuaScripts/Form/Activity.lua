-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local Activity_Test = Scripts[000000]

-- ****************************************************** --
--	Hàm này được gọi khi sự kiện được khởi tạo
--		activity: Activity - Sự kiện hiện tại
-- ****************************************************** --
function Activity_Test:OnInit(activity)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi sự kiện bắt đầu
--		activity: Activity - Sự kiện hiện tại
-- ****************************************************** --
function Activity_Test:OnStart(activity)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi liên tục chừng nào sự kiện còn tồn tại
--		activity: Activity - Sự kiện hiện tại
-- ****************************************************** --
function Activity_Test:OnTick(activity)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi sự kiện bị đóng
--		activity: Activity - Sự kiện hiện tại
-- ****************************************************** --
function Activity_Test:OnClose(activity)

	-- ************************** --
	
	-- ************************** --

end