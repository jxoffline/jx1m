-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local GrowPoint_Default = Scripts[300001]

-- ****************************************************** --
local ProductID = {
	[284] = { ItemID = 9490, ItemNumber = 1 },			-- Huyết Thảo
	[1989] = { ItemID = 11238, ItemNumber = 1 },		-- Thuoc Quy
	[1991] = { ItemID = 11225, ItemNumber = 1 },		-- Khoang
	[2005] = { ItemID = 11228, ItemNumber = 1 },		-- Hoa Anh Thảo
	[2006] = { ItemID = 11233, ItemNumber = 1 },		-- Linh Chi
	[2007] = { ItemID = 11234, ItemNumber = 1 },		-- Nhan Sam
	[2008] = { ItemID = 11235, ItemNumber = 1 },		-- Duong Quy
	[2009] = { ItemID = 11236, ItemNumber = 1 },		-- Tuyet Lien
	[2010] = { ItemID = 11227, ItemNumber = 1 },		-- Co Quan
	[2015] = { ItemID = 11252, ItemNumber = 1 },		-- Thanh Độc Thảo
	[2016] = { ItemID = 11253, ItemNumber = 1 },		-- Hàn Diêm 
	[2017] = { ItemID = 11254, ItemNumber = 1 },		-- Hoa Thi Vu 
	[2018] = { ItemID = 11259, ItemNumber = 1 },		-- Bi Bao 
}
-- ****************************************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào điểm thu thập, kiểm tra điều kiện có được thu thập hay không
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể thu thập, False nếu không thỏa mãn
-- ****************************************************** --
function GrowPoint_Default:OnPreCheckCondition(scene, growPoint, player)

	--**************************--
	--player:AddNotification("GrowPoint_Default => OnPreCheckCondition")
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
function GrowPoint_Default:OnActivateEachTick(scene, growPoint, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập hoàn tất
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Default:OnComplete(scene, growPoint, player)

	-- ************************** --
	player:AddNotification("Thu thập thành công!")
	-- ************************** --
	if not ProductID then 
		return
	end
	-- ************************** --
	local product = ProductID[growPoint:GetResID()]
	Player.AddItemLua(player, product.ItemID, product.ItemNumber, 0, 1)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập bị hủy bỏ
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Default:OnCancel(scene, growPoint, player)

	-- ************************** --
	--player:AddNotification("GrowPoint_Default => OnCancel")
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi quá trình thu thập thất bại
--		scene: Scene - Bản đồ hiện tại
--		growPoint: GrowPoint - Điểm thu thập tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function GrowPoint_Default:OnFaild(scene, growPoint, player)

	-- ************************** --
	player:AddNotification("Thu thập thất bại")
	-- ************************** --

end
