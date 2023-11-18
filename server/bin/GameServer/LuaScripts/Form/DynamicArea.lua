-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local DynamicArea_Test = Scripts[000000]

-- ****************************************************** --
--	Hàm này được gọi khi đối tượng bắt đầu tiến vào vùng ảnh hưởng của khu vực động
--		scene: Scene - Bản đồ hiện tại
--		dynArea: DynamicArea - Đối tượng khu vực động
--		obj: {Player, Monster} - Đối tượng tương ứng
-- ****************************************************** --
function DynamicArea_Test:OnEnter(scene, dynArea, obj)

	-- ************************** --

	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi đối tượng đứng trong vùng ảnh hưởng của khu vực động
--		scene: Scene - Bản đồ hiện tại
--		dynArea: DynamicArea - Đối tượng khu vực động
--		obj: {Player, Monster} - Đối tượng tương ứng
-- ****************************************************** --
function DynamicArea_Test:OnStayTick(scene, dynArea, obj)

	-- ************************** --

	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi đối tượng rời khỏi vùng ảnh hưởng của khu vực động
--		scene: Scene - Bản đồ hiện tại
--		dynArea: DynamicArea - Đối tượng khu vực động
--		obj: {Player, Monster} - Đối tượng tương ứng
-- ****************************************************** --
function DynamicArea_Test:OnLeave(scene, dynArea, obj)

	-- ************************** --

	-- ************************** --

end