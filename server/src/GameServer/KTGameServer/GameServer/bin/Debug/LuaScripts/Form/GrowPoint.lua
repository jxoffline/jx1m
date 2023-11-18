-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local GrowPoint_Test = Scripts[000000]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào điểm thu thập, kiểm tra điều kiện có được thu thập hay không
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể thu thập, False nếu không thỏa mãn
-- ****************************************************** --
function GrowPoint_Test:OnPreCheckCondition(scene, growPoint, player)

	-- ************************** --
	return true
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic liên tục khi thanh Progress Bar đang chạy thực thi thao tác với điểm thu thập
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Test:OnActivateEachTick(scene, growPoint, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập hoàn tất
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Test:OnComplete(scene, growPoint, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập bị hủy bỏ
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Test:OnCancel(scene, growPoint, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập thất bại
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Test:OnFaild(scene, growPoint, player)

	-- ************************** --
	
	-- ************************** --

end
