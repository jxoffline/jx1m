--[[
	SCRIPT AI quái vật mặc định
	Tác giả: Steven Huang
	Thời gian: 18/12/2020 
]]

-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local AIMonster_Default = Scripts[000000]


-- ========================================================= --
--	Hàm này gọi liên tục mỗi 0.5s một lần chừng nào quái còn sống và đang đuổi theo mục tiêu
--		- Lua_Scene scene: Bản đồ hiện tại
--		- Lua_Monster monster: Quái
--		- Lua_Object target: Mục tiêu
-- ========================================================= --
function AIMonster_Default:AITick(scene, monster, target)

	-- *********************************** --
	
	-- *********************************** --

end